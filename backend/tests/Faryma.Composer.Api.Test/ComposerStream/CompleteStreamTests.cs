using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Features.ComposerStream.Complete;
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
using Microsoft.Extensions.DependencyInjection;
using static Faryma.Composer.Api.Test.Infrastructure.DateTimeTestAssertions;

namespace Faryma.Composer.Api.Test.ComposerStream
{
    public sealed class CompleteStreamTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _completeRoute = "/api/ComposerStream/Complete";

        private static readonly DateTime _now = TruncateToMilliseconds(DateTime.UtcNow);
        private static readonly DateOnly _today = DateOnly.FromDateTime(_now);

        [Theory]
        [InlineData(ComposerStreamType.Donation)]
        [InlineData(ComposerStreamType.Debt)]
        [InlineData(ComposerStreamType.Charity)]
        public async Task Composer_completes_live_stream_when_no_order_is_in_progress(ComposerStreamType type)
        {
            DateTime startedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                type: type,
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            CompleteResponse body = await ReadCompleteResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(_today, body.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Completed, body.ComposerStream.Status);
            Assert.Equal(type, body.ComposerStream.Type);
            AssertSameInstant(startedAt, body.ComposerStream.StartedAt);
            AssertSameInstant(_now, body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            AssertSameInstant(_now, persisted.CompletedAt);

            TestOrderQueueNotificationService notifications = timed.Services.GetOrderQueueNotifications();
            Assert.Equal(1, notifications.UpdateCount);
            Assert.Equal(OrderQueueUpdateType.StreamCompleted, notifications.Snapshots.Single().OrderQueueUpdateType);
        }

        [Fact]
        public async Task Repeat_complete_of_completed_stream_returns_it_unchanged()
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

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            CompleteResponse body = await ReadCompleteResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(ComposerStreamStatus.Completed, body.ComposerStream.Status);
            AssertSameInstant(startedAt, body.ComposerStream.StartedAt);
            AssertSameInstant(completedAt, body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            AssertSameInstant(completedAt, persisted.CompletedAt);
            Assert.Equal(0, timed.Services.GetOrderQueueNotifications().UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_complete_stream_when_order_is_in_progress()
        {
            DateTime startedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            ReviewOrderEntity inProgress = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                processingStreamId: stream.Id,
                nickname: "nick-in-progress",
                status: ReviewOrderStatus.InProgress,
                inProgressAt: _now.AddMinutes(-20));
            ReviewOrderEntity pending = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                nickname: "nick-pending",
                status: ReviewOrderStatus.Pending);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            string? inProgressMessage = await response.AssertApiErrorAsync(nameof(ComposerStreamException));
            Assert.Contains("невозможно завершить стрим", inProgressMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("в работе", inProgressMessage, StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);

            ReviewOrderEntity persistedInProgress = await timed.Services.GetOrderAsync(inProgress.Id, TestContext.Current.CancellationToken);
            ReviewOrderEntity persistedPending = await timed.Services.GetOrderAsync(pending.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ReviewOrderStatus.InProgress, persistedInProgress.Status);
            Assert.Equal(ReviewOrderStatus.Pending, persistedPending.Status);
            Assert.Equal(0, timed.Services.GetOrderQueueNotifications().UpdateCount);
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.AwaitingPayment)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Unprocessed_orders_stay_in_queue_after_complete(ReviewOrderStatus status)
        {
            DateTime startedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            await data.CreateStreamAsync(eventDate: _today.AddDays(1));
            ReviewOrderEntity order = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                nickname: $"nick-{status}",
                status: status);
            ReviewOrderEntity frozen = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                nickname: $"nick-frozen-{status}",
                status: ReviewOrderStatus.Pending,
                isFrozen: true);
            await ReloadQueueAsync(timed);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            CompleteResponse body = await ReadCompleteResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ComposerStreamStatus.Completed, body.ComposerStream.Status);
            AssertSameInstant(_now, body.ComposerStream.CompletedAt);

            ReviewOrderEntity persistedOrder = await timed.Services.GetOrderAsync(order.Id, TestContext.Current.CancellationToken);
            ReviewOrderEntity persistedFrozen = await timed.Services.GetOrderAsync(frozen.Id, TestContext.Current.CancellationToken);
            Assert.Equal(status, persistedOrder.Status);
            Assert.False(persistedOrder.IsFrozen);
            Assert.Equal(ReviewOrderStatus.Pending, persistedFrozen.Status);
            Assert.True(persistedFrozen.IsFrozen);

            OrderQueueSnapshot snapshot = timed.Services.GetOrderQueueNotifications().Snapshots.Single();
            Assert.Equal(OrderQueueUpdateType.StreamCompleted, snapshot.OrderQueueUpdateType);
            Assert.NotEqual(OrderActivityStatus.Removed, GetActivity(snapshot, order.Id));
            Assert.Equal(OrderActivityStatus.Frozen, GetActivity(snapshot, frozen.Id));

            OrderQueueSnapshot current = await GetCurrentQueueAsync(timed);
            Assert.Contains(order.Id, current.Positions.Select(x => x.Order.Id));
            Assert.Contains(frozen.Id, current.Positions.Select(x => x.Order.Id));
        }

        [Fact]
        public async Task Completed_orders_leave_queue_after_stream_complete()
        {
            DateTime startedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            ReviewOrderEntity completed = await data.CreateReviewOrderAsync(
                creationStreamId: stream.Id,
                processingStreamId: stream.Id,
                nickname: "nick-completed",
                status: ReviewOrderStatus.Completed,
                queueCategory: QueueCategory.Donation,
                completedAt: _now.AddHours(-1));
            await ReloadQueueAsync(timed);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            CompleteResponse body = await ReadCompleteResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ComposerStreamStatus.Completed, body.ComposerStream.Status);

            ReviewOrderEntity persisted = await timed.Services.GetOrderAsync(completed.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ReviewOrderStatus.Completed, persisted.Status);
            Assert.Equal(stream.Id, persisted.ProcessingStreamId);

            OrderQueueSnapshot snapshot = timed.Services.GetOrderQueueNotifications().Snapshots.Single();
            Assert.Equal(OrderQueueUpdateType.StreamCompleted, snapshot.OrderQueueUpdateType);
            Assert.Equal(OrderActivityStatus.Removed, GetActivity(snapshot, completed.Id));
            OrderQueueSnapshot current = await GetCurrentQueueAsync(timed);
            Assert.DoesNotContain(completed.Id, current.Positions.Select(x => x.Order.Id));
        }

        [Theory]
        [InlineData(ComposerStreamStatus.Planned)]
        [InlineData(ComposerStreamStatus.Canceled)]
        public async Task Composer_cannot_complete_stream_that_is_not_live(ComposerStreamStatus status)
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today, status: status);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            string? message = await response.AssertApiErrorAsync(nameof(ComposerStreamException));
            Assert.Contains("невозможно завершить стрим", message, StringComparison.OrdinalIgnoreCase);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(status, persisted.Status);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);
            Assert.Equal(0, timed.Services.GetOrderQueueNotifications().UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_complete_missing_stream()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, 999_999);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            await response.AssertApiErrorAsync(nameof(NotFoundException));
            Assert.Equal(0, await timed.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, timed.Services.GetOrderQueueNotifications().UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_complete_when_id_is_invalid()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostCompleteAsync(client, 0);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await timed.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, timed.Services.GetOrderQueueNotifications().UpdateCount);
        }

        [Fact]
        public async Task Anonymous_request_gets_401()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await PostCompleteAsync(client, 1);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_composer_role_gets_403()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await PostCompleteAsync(client, 1);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static Task<HttpClient> CreateComposerClientAsync(CustomWebApplicationFactory app) =>
            app.CreateAdminBearerClientAsync(new TestAuthUserSeed
            {
                UserName = "composer_complete_stream",
                Password = "TestComposerPass123!",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

        private static Task<HttpResponseMessage> PostCompleteAsync(HttpClient client, long composerStreamId) =>
            client.PostAsJsonAsync(
                _completeRoute,
                new CompleteRequest
                {
                    ComposerStreamId = composerStreamId,
                },
                TestJsonSerializerOptions.Web,
                TestContext.Current.CancellationToken);

        private static async Task<CompleteResponse> ReadCompleteResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<CompleteResponse>(TestJsonSerializerOptions.Web, TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ завершения стрима");
        }

        private static Task ReloadQueueAsync(CustomWebApplicationFactory app) =>
            app.Services.GetRequiredService<OrderQueueService>().Initialize();

        private static Task<OrderQueueSnapshot> GetCurrentQueueAsync(CustomWebApplicationFactory app) =>
            app.Services.GetRequiredService<OrderQueueService>().GetQueueSnapshot();

        private static OrderActivityStatus GetActivity(OrderQueueSnapshot snapshot, long orderId)
        {
            OrderPosition position = Assert.Single(snapshot.Positions, x => x.Order.Id == orderId);
            return position.PositionHistory.Current.ActivityStatus;
        }
    }
}
