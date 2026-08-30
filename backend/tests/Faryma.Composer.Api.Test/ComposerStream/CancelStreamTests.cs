using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Api.Features.ComposerStream.Cancel;
using Faryma.Composer.Api.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Models;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure.Features.OrderQueue;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Api.Test.ComposerStream
{
    public sealed class CancelStreamTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _cancelRoute = "/api/ComposerStream/Cancel";
        private const string _findLiveAndPlannedRoute = "/api/ComposerStream/FindLiveAndPlanned";

        private static readonly DateTime _now = TruncateToMilliseconds(DateTime.UtcNow);
        private static readonly DateOnly _today = DateOnly.FromDateTime(_now);

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };

        [Theory]
        [InlineData(ComposerStreamType.Donation)]
        [InlineData(ComposerStreamType.Debt)]
        [InlineData(ComposerStreamType.Charity)]
        public async Task Composer_cancels_planned_stream_when_no_active_created_orders(ComposerStreamType type)
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today, type: type);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            CancelResponse body = await ReadCancelResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(_today, body.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Canceled, body.ComposerStream.Status);
            Assert.Equal(type, body.ComposerStream.Type);
            Assert.Null(body.ComposerStream.StartedAt);
            Assert.Null(body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
            Assert.Equal(_today, persisted.EventDate);
            Assert.Equal(type, persisted.Type);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);

            TestOrderQueueNotificationService notifications = GetNotifications(timed);
            Assert.Equal(1, notifications.UpdateCount);
            Assert.Equal(OrderQueueUpdateType.StreamCanceled, notifications.Snapshots.Single().OrderQueueUpdateType);
        }

        [Fact]
        public async Task Cancel_recalculates_queue_and_drops_stream_from_live_and_planned_list()
        {
            DateOnly nextDate = _today.AddDays(1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today);
            ComposerStreamEntity next = await data.CreateStreamAsync(eventDate: nextDate);
            ReviewOrderEntity order = await data.CreateReviewOrderAsync(
                creationStreamId: next.Id,
                nickname: "nick-next-stream",
                status: ReviewOrderStatus.Pending);
            await ReloadQueueAsync(timed);
            OrderQueueSnapshot before = await GetCurrentQueueAsync(timed);
            using HttpClient client = await CreateComposerClientAsync(timed);

            Assert.Equal(OrderActivityStatus.Scheduled, GetActivity(before, order.Id));
            Assert.Equal(_today, await GetNearestStreamDateAsync(timed));

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            CancelResponse body = await ReadCancelResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();
            FindLiveAndPlannedResponse liveAndPlanned = await GetLiveAndPlannedAsync(client);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(ComposerStreamStatus.Canceled, body.ComposerStream.Status);

            ReviewOrderEntity persistedOrder = await timed.Services.GetOrderAsync(order.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ReviewOrderStatus.Pending, persistedOrder.Status);
            Assert.Equal(next.Id, persistedOrder.CreationStreamId);

            OrderQueueSnapshot snapshot = GetNotifications(timed).Snapshots.Single();
            Assert.Equal(OrderQueueUpdateType.StreamCanceled, snapshot.OrderQueueUpdateType);
            Assert.Equal(OrderActivityStatus.Active, GetActivity(snapshot, order.Id));
            Assert.Equal(nextDate, await GetNearestStreamDateAsync(timed));

            Assert.DoesNotContain(stream.Id, liveAndPlanned.Streams.Select(x => x.Id));
            Assert.Contains(next.Id, liveAndPlanned.Streams.Select(x => x.Id));
        }

        [Fact]
        public async Task Composer_can_cancel_past_planned_stream()
        {
            DateOnly yesterday = _today.AddDays(-1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: yesterday);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            CancelResponse body = await ReadCancelResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ComposerStreamStatus.Canceled, body.ComposerStream.Status);
            Assert.Equal(yesterday, body.ComposerStream.EventDate);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
            Assert.Equal(yesterday, persisted.EventDate);
            Assert.Equal(1, GetNotifications(timed).UpdateCount);
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task Completed_and_canceled_created_orders_do_not_block_cancel(ReviewOrderStatus status)
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today);
            ComposerStreamEntity? processing = status == ReviewOrderStatus.Completed
                ? await data.CreateStreamAsync(
                    eventDate: _today.AddDays(-1),
                    status: ComposerStreamStatus.Live,
                    startedAt: _now.AddHours(-2))
                : null;
            ReviewOrderEntity order = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                processingStreamId: processing?.Id,
                nickname: $"nick-{status}",
                status: status,
                completedAt: status == ReviewOrderStatus.Completed ? _now.AddHours(-1) : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? _now.AddHours(-1) : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "тестовая отмена" : null);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            CancelResponse body = await ReadCancelResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ComposerStreamStatus.Canceled, body.ComposerStream.Status);

            ReviewOrderEntity persistedOrder = await timed.Services.GetOrderAsync(order.Id, TestContext.Current.CancellationToken);
            Assert.Equal(status, persistedOrder.Status);
            Assert.Equal(order.IsFrozen, persistedOrder.IsFrozen);
            Assert.Equal(order.ProcessingStreamId, persistedOrder.ProcessingStreamId);
            Assert.Equal(order.CancelReason, persistedOrder.CancelReason);
            Assert.Equal(1, GetNotifications(timed).UpdateCount);
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.AwaitingPayment)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Composer_cannot_cancel_stream_when_active_created_order_exists(ReviewOrderStatus status)
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today);
            ReviewOrderEntity order = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                nickname: $"nick-active-{status}",
                status: status);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Contains("невозможно отменить стрим", json.RootElement.GetProperty("Message").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("активные заказы", json.RootElement.GetProperty("Message").GetString(), StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Planned, persisted.Status);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);

            ReviewOrderEntity persistedOrder = await timed.Services.GetOrderAsync(order.Id, TestContext.Current.CancellationToken);
            Assert.Equal(status, persistedOrder.Status);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Frozen_active_created_order_still_blocks_cancel()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today);
            ReviewOrderEntity order = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                nickname: "nick-frozen-pending",
                status: ReviewOrderStatus.Pending,
                isFrozen: true);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Contains("активные заказы", json.RootElement.GetProperty("Message").GetString(), StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            ReviewOrderEntity persistedOrder = await timed.Services.GetOrderAsync(order.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Planned, persisted.Status);
            Assert.Equal(ReviewOrderStatus.Pending, persistedOrder.Status);
            Assert.True(persistedOrder.IsFrozen);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Repeat_cancel_of_canceled_stream_returns_it_unchanged()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Canceled);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            CancelResponse body = await ReadCancelResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(ComposerStreamStatus.Canceled, body.ComposerStream.Status);
            Assert.Equal(_today, body.ComposerStream.EventDate);
            Assert.Null(body.ComposerStream.StartedAt);
            Assert.Null(body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
            Assert.Equal(_today, persisted.EventDate);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_cancel_live_stream()
        {
            DateTime startedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Contains("невозможно отменить стрим", json.RootElement.GetProperty("Message").GetString(), StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_cancel_completed_stream()
        {
            DateTime startedAt = _now.AddHours(-3);
            DateTime completedAt = _now.AddHours(-1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Completed,
                startedAt: startedAt,
                completedAt: completedAt);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Contains("невозможно отменить стрим", json.RootElement.GetProperty("Message").GetString(), StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            AssertSameInstant(completedAt, persisted.CompletedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_cancel_missing_stream()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, 999_999);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(NotFoundException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Equal(0, await timed.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_cancel_when_id_is_invalid()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCancelAsync(client, 0);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await timed.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Anonymous_request_gets_401()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await PostCancelAsync(client, 1);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_composer_role_gets_403()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await PostCancelAsync(client, 1);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static Task<HttpClient> CreateComposerClientAsync(CustomWebApplicationFactory app) =>
            app.CreateAdminBearerClientAsync(new TestAuthUserSeed
            {
                UserName = "composer_cancel_stream",
                Password = "TestComposerPass123!",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

        private static Task<HttpResponseMessage> PostCancelAsync(HttpClient client, long composerStreamId) =>
            client.PostAsJsonAsync(
                _cancelRoute,
                new CancelRequest
                {
                    ComposerStreamId = composerStreamId,
                },
                _jsonOptions,
                TestContext.Current.CancellationToken);

        private static async Task<CancelResponse> ReadCancelResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<CancelResponse>(_jsonOptions, TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ отмены стрима");
        }

        private static async Task<FindLiveAndPlannedResponse> GetLiveAndPlannedAsync(HttpClient client)
        {
            using HttpResponseMessage response = await client.GetAsync(_findLiveAndPlannedRoute, TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<FindLiveAndPlannedResponse>(_jsonOptions, TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать список текущего и запланированных стримов");
        }

        private static Task ReloadQueueAsync(CustomWebApplicationFactory app) =>
            app.Services.GetRequiredService<OrderQueueService>().Initialize();

        private static TestOrderQueueNotificationService GetNotifications(CustomWebApplicationFactory app) =>
            (TestOrderQueueNotificationService)app.Services.GetRequiredService<IOrderQueueNotificationService>();

        private static Task<OrderQueueSnapshot> GetCurrentQueueAsync(CustomWebApplicationFactory app) =>
            app.Services.GetRequiredService<OrderQueueService>().GetQueueSnapshot();

        private static Task<DateOnly> GetNearestStreamDateAsync(CustomWebApplicationFactory app) =>
            app.Services.RunInScopeAsync(scoped =>
                scoped.GetRequiredService<OrderQueueStore>().FindNearestStreamDate());

        private static OrderActivityStatus GetActivity(OrderQueueSnapshot snapshot, long orderId)
        {
            OrderPosition position = Assert.Single(snapshot.Positions, x => x.Order.Id == orderId);
            return position.PositionHistory.Current.ActivityStatus;
        }

        private static void AssertSameInstant(DateTime? expected, DateTime? actual)
        {
            if (expected is null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(TruncateToMilliseconds(expected.Value), TruncateToMilliseconds(actual.Value));
        }

        private static DateTime TruncateToMilliseconds(DateTime value) =>
            new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Utc);
    }
}
