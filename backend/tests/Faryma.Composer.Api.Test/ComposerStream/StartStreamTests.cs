using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Api.Features.ComposerStream.Start;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Api.Test.ComposerStream
{
    public sealed class StartStreamTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _startRoute = "/api/ComposerStream/Start";

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
        public async Task Composer_starts_planned_stream_when_event_date_is_today(ComposerStreamType type)
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today, type: type);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, stream.Id);
            StartResponse body = await ReadStartResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(_today, body.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Live, body.ComposerStream.Status);
            Assert.Equal(type, body.ComposerStream.Type);
            AssertSameInstant(_now, body.ComposerStream.StartedAt);
            Assert.Null(body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            AssertSameInstant(_now, persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);

            TestOrderQueueNotificationService notifications = GetNotifications(timed);
            Assert.Equal(1, notifications.UpdateCount);
            Assert.Equal(OrderQueueUpdateType.StreamStarted, notifications.Snapshots.Single().OrderQueueUpdateType);
        }

        [Fact]
        public async Task Repeat_start_of_live_stream_returns_it_unchanged()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: _today);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage firstResponse = await PostStartAsync(client, stream.Id);
            StartResponse first = await ReadStartResponseAsync(firstResponse);
            using HttpResponseMessage secondResponse = await PostStartAsync(client, stream.Id);
            StartResponse second = await ReadStartResponseAsync(secondResponse);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.Equal(stream.Id, second.ComposerStream.Id);
            Assert.Equal(ComposerStreamStatus.Live, second.ComposerStream.Status);
            AssertSameInstant(first.ComposerStream.StartedAt, second.ComposerStream.StartedAt);
            AssertSameInstant(_now, second.ComposerStream.StartedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            AssertSameInstant(_now, persisted.StartedAt);

            TestOrderQueueNotificationService notifications = GetNotifications(timed);
            Assert.Equal(1, notifications.UpdateCount);
            Assert.Equal(OrderQueueUpdateType.StreamStarted, notifications.Snapshots.Single().OrderQueueUpdateType);
        }

        [Fact]
        public async Task Repeat_start_of_live_stream_stays_idempotent_when_event_date_is_no_longer_today()
        {
            DateTime startedAt = _now.AddDays(-1).AddHours(8);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today.AddDays(-1),
                status: ComposerStreamStatus.Live,
                startedAt: startedAt);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, stream.Id);
            StartResponse body = await ReadStartResponseAsync(response);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(stream.Id, body.ComposerStream.Id);
            Assert.Equal(ComposerStreamStatus.Live, body.ComposerStream.Status);
            AssertSameInstant(startedAt, body.ComposerStream.StartedAt);

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public async Task Composer_cannot_start_planned_stream_when_event_date_is_not_today(int dayOffset)
        {
            DateOnly eventDate = _today.AddDays(dayOffset);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(eventDate: eventDate);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Planned, persisted.Status);
            Assert.Null(persisted.StartedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_start_stream_when_another_is_live()
        {
            DateTime liveStartedAt = _now.AddHours(-2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity live = await data.CreateStreamAsync(
                eventDate: _today.AddDays(-1),
                status: ComposerStreamStatus.Live,
                startedAt: liveStartedAt);
            ComposerStreamEntity planned = await data.CreateStreamAsync(eventDate: _today);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, planned.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());

            ComposerStreamEntity persistedLive = await timed.Services.GetStreamAsync(live.Id, TestContext.Current.CancellationToken);
            ComposerStreamEntity persistedPlanned = await timed.Services.GetStreamAsync(planned.Id, TestContext.Current.CancellationToken);
            Assert.Equal(ComposerStreamStatus.Live, persistedLive.Status);
            AssertSameInstant(liveStartedAt, persistedLive.StartedAt);
            Assert.Equal(ComposerStreamStatus.Planned, persistedPlanned.Status);
            Assert.Null(persistedPlanned.StartedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Theory]
        [InlineData(ComposerStreamStatus.Completed)]
        [InlineData(ComposerStreamStatus.Canceled)]
        public async Task Composer_cannot_start_stream_in_terminal_status(ComposerStreamStatus status)
        {
            DateTime? startedAt = status == ComposerStreamStatus.Completed ? _now.AddHours(-3) : null;
            DateTime? completedAt = status == ComposerStreamStatus.Completed ? _now.AddHours(-1) : null;
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            TestDataBuilder data = new(timed.Services);
            ComposerStreamEntity stream = await data.CreateStreamAsync(
                eventDate: _today,
                status: status,
                startedAt: startedAt,
                completedAt: completedAt);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, stream.Id);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());

            ComposerStreamEntity persisted = await timed.Services.GetStreamAsync(stream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(status, persisted.Status);
            AssertSameInstant(startedAt, persisted.StartedAt);
            AssertSameInstant(completedAt, persisted.CompletedAt);
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_start_missing_stream()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, 999_999);
            await timed.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(NotFoundException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Equal(0, await timed.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(timed).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_start_when_id_is_invalid()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            CustomWebApplicationFactory timed = app.WithFixedDateTime(_now);
            using HttpClient client = await CreateComposerClientAsync(timed);

            using HttpResponseMessage response = await PostStartAsync(client, 0);
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

            using HttpResponseMessage response = await PostStartAsync(client, 1);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_composer_role_gets_403()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await PostStartAsync(client, 1);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static Task<HttpClient> CreateComposerClientAsync(CustomWebApplicationFactory app) =>
            app.CreateAdminBearerClientAsync(new TestAuthUserSeed
            {
                UserName = "composer_start_stream",
                Password = "TestComposerPass123!",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

        private static Task<HttpResponseMessage> PostStartAsync(HttpClient client, long composerStreamId) =>
            client.PostAsJsonAsync(
                _startRoute,
                new StartRequest
                {
                    ComposerStreamId = composerStreamId,
                },
                _jsonOptions,
                TestContext.Current.CancellationToken);

        private static async Task<StartResponse> ReadStartResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<StartResponse>(_jsonOptions, TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ запуска стрима");
        }

        private static TestOrderQueueNotificationService GetNotifications(CustomWebApplicationFactory app) =>
            (TestOrderQueueNotificationService)app.Services.GetRequiredService<IOrderQueueNotificationService>();

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
