using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Features.ComposerStream.Create;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Api.Test.Features.ComposerStream
{
    public sealed class CreateStreamTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _route = "/api/ComposerStream/Create";

        [Fact]
        public async Task Composer_creates_planned_stream_for_future_date()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUsers users = await app.SeedUsersAsync(ct: TestContext.Current.CancellationToken);
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            DateOnly eventDate = Today().AddDays(7);

            using HttpResponseMessage response = await CreateAsync(client, eventDate, ComposerStreamType.Donation);
            CreateResponse result = await ReadResponseAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(result.ComposerStream.Id > 0);
            Assert.Equal(eventDate, result.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamType.Donation, result.ComposerStream.Type);
            Assert.Equal(ComposerStreamStatus.Planned, result.ComposerStream.Status);
            Assert.Null(result.ComposerStream.StartedAt);
            Assert.Null(result.ComposerStream.CompletedAt);

            ComposerStreamEntity stored = await app.Services.GetStreamAsync(
                result.ComposerStream.Id, TestContext.Current.CancellationToken);

            Assert.Equal(eventDate, stored.EventDate);
            Assert.Equal(ComposerStreamType.Donation, stored.Type);
            Assert.Equal(ComposerStreamStatus.Planned, stored.Status);
            Assert.Null(stored.StartedAt);
            Assert.Null(stored.CompletedAt);
            Assert.Equal(users.Admin.UserId, stored.CreatedByUserId);
        }

        [Fact]
        public async Task Composer_creates_stream_for_today()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            DateOnly today = Today();

            using HttpResponseMessage response = await CreateAsync(client, today, ComposerStreamType.Charity);
            CreateResponse result = await ReadResponseAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(today, result.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Planned, result.ComposerStream.Status);
        }

        [Fact]
        public async Task Create_rejects_past_event_date()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(-1), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Create_rejects_unspecified_stream_type()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(1), ComposerStreamType.Unspecified);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Create_rejects_stream_type_outside_enum()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(1), (ComposerStreamType)999);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Create_rejects_occupied_event_date_when_existing_stream_canceled()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUsers users = await app.SeedUsersAsync(ct: TestContext.Current.CancellationToken);
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            DateOnly eventDate = Today().AddDays(7);
            TestDataBuilder data = new(app.Services);
            ComposerStreamEntity existing = await data.CreateStreamAsync(
                createdByUserId: users.Admin.UserId,
                eventDate: eventDate,
                type: ComposerStreamType.Debt,
                status: ComposerStreamStatus.Canceled);
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(client, eventDate, ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            await response.AssertApiErrorAsync("composer_stream");
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));

            ComposerStreamEntity stored = await app.Services.GetStreamAsync(
                existing.Id, TestContext.Current.CancellationToken);

            Assert.Equal(ComposerStreamStatus.Canceled, stored.Status);
            Assert.Equal(ComposerStreamType.Debt, stored.Type);
        }

        [Fact]
        public async Task Create_forbids_authenticated_user_without_composer_role()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(1), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Create_forbids_anonymous_user()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = app.CreateAnonymousClient();
            int streamsBefore = await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(1), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(streamsBefore, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task Successful_create_notifies_order_queue()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);
            TestOrderQueueNotificationService notifications = app.Services.GetOrderQueueNotifications();
            int updatesBefore = notifications.UpdateCount;

            using HttpResponseMessage response = await CreateAsync(
                client, Today().AddDays(7), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(updatesBefore + 1, notifications.UpdateCount);

            OrderQueueSnapshot snapshot = Assert.Single(notifications.Snapshots.Skip(updatesBefore));
            Assert.Equal(OrderQueueUpdateType.StreamCreated, snapshot.OrderQueueUpdateType);
        }

        private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

        private static Task<HttpResponseMessage> CreateAsync(
            HttpClient client,
            DateOnly eventDate,
            ComposerStreamType type) =>
            client.PostAsJsonAsync(
                _route,
                new CreateRequest
                {
                    EventDate = eventDate,
                    Type = type,
                },
                TestJsonSerializerOptions.Web,
                TestContext.Current.CancellationToken);

        private static async Task<CreateResponse> ReadResponseAsync(HttpResponseMessage response)
        {
            CreateResponse? result = await response.Content.ReadFromJsonAsync<CreateResponse>(
                TestJsonSerializerOptions.Web,
                TestContext.Current.CancellationToken);

            return result ?? throw new InvalidOperationException("Ответ создания стрима оказался пустым");
        }
    }
}
