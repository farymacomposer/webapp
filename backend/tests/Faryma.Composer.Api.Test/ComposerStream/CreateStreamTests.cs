using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Api.Features.ComposerStream.Create;
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
    public sealed class CreateStreamTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _createRoute = "/api/ComposerStream/Create";

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };

        [Theory]
        [InlineData(ComposerStreamType.Donation)]
        [InlineData(ComposerStreamType.Debt)]
        [InlineData(ComposerStreamType.Charity)]
        public async Task Composer_creates_planned_stream_when_date_is_free(ComposerStreamType type)
        {
            DateOnly eventDate = UtcToday().AddDays(16);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await PostCreateAsync(client, eventDate, type);
            CreateResponse body = await ReadCreateResponseAsync(response);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body.ComposerStream.Id > 0);
            Assert.Equal(eventDate, body.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Planned, body.ComposerStream.Status);
            Assert.Equal(type, body.ComposerStream.Type);
            Assert.Null(body.ComposerStream.StartedAt);
            Assert.Null(body.ComposerStream.CompletedAt);

            ComposerStreamEntity persisted = await app.Services.GetStreamAsync(body.ComposerStream.Id, TestContext.Current.CancellationToken);
            Assert.Equal(eventDate, persisted.EventDate);
            Assert.Equal(ComposerStreamStatus.Planned, persisted.Status);
            Assert.Equal(type, persisted.Type);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);
            Assert.Equal(1, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));

            TestOrderQueueNotificationService notifications = GetNotifications(app);
            Assert.Equal(1, notifications.UpdateCount);
            Assert.Equal(OrderQueueUpdateType.StreamCreated, notifications.Snapshots.Single().OrderQueueUpdateType);
        }

        [Fact]
        public async Task Composer_can_create_stream_on_today_utc()
        {
            DateOnly today = UtcToday();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await PostCreateAsync(client, today, ComposerStreamType.Donation);
            CreateResponse body = await ReadCreateResponseAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(today, body.ComposerStream.EventDate);
            Assert.Equal(ComposerStreamStatus.Planned, body.ComposerStream.Status);
        }

        [Fact]
        public async Task Composer_cannot_create_stream_on_past_utc_date()
        {
            DateOnly yesterday = UtcToday().AddDays(-1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await PostCreateAsync(client, yesterday, ComposerStreamType.Donation);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(app).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_create_stream_when_type_is_unspecified()
        {
            DateOnly eventDate = UtcToday().AddDays(1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await PostCreateAsync(client, eventDate, ComposerStreamType.Unspecified);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(app).UpdateCount);
        }

        [Fact]
        public async Task Composer_cannot_create_stream_when_type_is_invalid()
        {
            DateOnly eventDate = UtcToday().AddDays(1);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await client.PostAsJsonAsync(
                _createRoute,
                new { EventDate = eventDate, Type = 99 },
                TestContext.Current.CancellationToken);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(app).UpdateCount);
        }

        [Theory]
        [InlineData(ComposerStreamStatus.Canceled)]
        [InlineData(ComposerStreamStatus.Completed)]
        [InlineData(ComposerStreamStatus.Live)]
        [InlineData(ComposerStreamStatus.Planned)]
        public async Task Composer_cannot_create_stream_when_date_is_already_taken(ComposerStreamStatus existingStatus)
        {
            DateOnly eventDate = UtcToday().AddDays(2);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            TestDataBuilder data = new(app.Services);
            await data.CreateStreamAsync(eventDate: eventDate, status: existingStatus);
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage response = await PostCreateAsync(client, eventDate, ComposerStreamType.Charity);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Contains("уже существует", json.RootElement.GetProperty("Message").GetString(), StringComparison.Ordinal);
            Assert.Equal(1, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, GetNotifications(app).UpdateCount);
        }

        [Fact]
        public async Task Repeat_create_on_same_date_is_rejected_and_does_not_return_existing()
        {
            DateOnly eventDate = UtcToday().AddDays(3);
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await CreateComposerClientAsync(app);

            using HttpResponseMessage firstResponse = await PostCreateAsync(client, eventDate, ComposerStreamType.Donation);
            CreateResponse first = await ReadCreateResponseAsync(firstResponse);
            using HttpResponseMessage secondResponse = await PostCreateAsync(client, eventDate, ComposerStreamType.Donation);
            await app.Services.DrainOrderQueueEventsAsync();

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(AppException.StatusCode, (int)secondResponse.StatusCode);
            using var json = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(nameof(ComposerStreamException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.DoesNotContain(first.ComposerStream.Id.ToString(), json.RootElement.GetProperty("Message").GetString(), StringComparison.Ordinal);
            Assert.Equal(1, await app.Services.CountStreamsAsync(TestContext.Current.CancellationToken));
            Assert.Equal(1, GetNotifications(app).UpdateCount);
        }

        [Fact]
        public async Task Anonymous_request_gets_401()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await PostCreateAsync(client, UtcToday().AddDays(1), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_composer_role_gets_403()
        {
            await using CustomWebApplicationFactory app = CustomWebApplicationFactory.Create();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await PostCreateAsync(client, UtcToday().AddDays(1), ComposerStreamType.Donation);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static DateOnly UtcToday() => DateOnly.FromDateTime(DateTime.UtcNow);

        private static Task<HttpClient> CreateComposerClientAsync(CustomWebApplicationFactory app) =>
            app.CreateAdminBearerClientAsync(new TestAuthUserSeed
            {
                UserName = "composer_create_stream",
                Password = "TestComposerPass123!",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

        private static Task<HttpResponseMessage> PostCreateAsync(
            HttpClient client,
            DateOnly eventDate,
            ComposerStreamType type) =>
            client.PostAsJsonAsync(
                _createRoute,
                new CreateRequest
                {
                    EventDate = eventDate,
                    Type = type,
                },
                _jsonOptions,
                TestContext.Current.CancellationToken);

        private static async Task<CreateResponse> ReadCreateResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<CreateResponse>(_jsonOptions, TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ создания стрима");
        }

        private static TestOrderQueueNotificationService GetNotifications(CustomWebApplicationFactory app) =>
            (TestOrderQueueNotificationService)app.Services.GetRequiredService<IOrderQueueNotificationService>();
    }
}
