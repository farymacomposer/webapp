using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Contracts.Api;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Faryma.Composer.Api.Test.Common
{
    public sealed class IdempotentAttributeTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _route = "/api/_test/idempotency";

        [Fact]
        public async Task IdempotentEndpoint_ReplaysStoredResponse_ForSameIdempotencyKey()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "same-payload");

            using HttpResponseMessage firstResponse = await SendAsync(client, idempotencyKey, request);
            using HttpResponseMessage secondResponse = await SendAsync(client, idempotencyKey, request);

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse second = await ReadResponseAsync(secondResponse);

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.Equal(first, second);
            Assert.Equal(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [Fact]
        public async Task IdempotentEndpoint_ExecutesOnce_ForConcurrentSameIdempotencyKey()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "same-payload", DelayMilliseconds: 200);

            Task<HttpResponseMessage> firstTask = SendAsync(client, idempotencyKey, request);
            Task<HttpResponseMessage> secondTask = SendAsync(client, idempotencyKey, request);
            HttpResponseMessage[] responses = await Task.WhenAll(firstTask, secondTask);

            using HttpResponseMessage firstResponse = responses[0];
            using HttpResponseMessage secondResponse = responses[1];

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse second = await ReadResponseAsync(secondResponse);

            Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
            Assert.Equal(first, second);
            Assert.Equal(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [Fact]
        public async Task IdempotentEndpoint_ReturnsConflict_WhenSameKeyUsesDifferentPayload()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();
            Guid idempotencyKey = Guid.NewGuid();

            using HttpResponseMessage firstResponse = await SendAsync(client, idempotencyKey, new TestIdempotencyRequest(scenario, "first"));
            using HttpResponseMessage secondResponse = await SendAsync(client, idempotencyKey, new TestIdempotencyRequest(scenario, "second"));

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
            Assert.Equal(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [Fact]
        public async Task IdempotentEndpoint_DoesNotStoreIdempotencyRecord_WhenActionFails()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "retry-after-error");
            TestIdempotencyController.FailNext(scenario);

            using HttpResponseMessage failedResponse = await SendAsync(client, idempotencyKey, request);
            using HttpResponseMessage retryResponse = await SendAsync(client, idempotencyKey, request);

            TestIdempotencyResponse retry = await ReadResponseAsync(retryResponse);

            Assert.Equal(666, (int)failedResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, retryResponse.StatusCode);
            Assert.Equal(new TestIdempotencyResponse(scenario, "retry-after-error", 1), retry);
            Assert.Equal(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [Fact]
        public async Task IdempotentEndpoint_ExecutesAgain_WhenIdempotencyKeyExpired()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            Guid idempotencyKey = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            TestIdempotencyRequest request = new(scenario, "expired-key");

            CustomWebApplicationFactory firstApp = CreateAppWithNow(app, now);
            using HttpClient firstClient = await firstApp.CreateBrowserUserClientAsync();
            using HttpResponseMessage firstResponse = await SendAsync(firstClient, idempotencyKey, request);

            CustomWebApplicationFactory laterApp = CreateAppWithNow(app, now.AddHours(2));
            using HttpClient laterClient = await laterApp.CreateBrowserUserClientAsync();
            using HttpResponseMessage laterResponse = await SendAsync(laterClient, idempotencyKey, request);

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse later = await ReadResponseAsync(laterResponse);

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, laterResponse.StatusCode);
            Assert.Equal(1, first.Executions);
            Assert.Equal(2, later.Executions);
            Assert.Equal(2, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [Fact]
        public async Task IdempotentEndpoint_Throws_WhenActionReturnsUnsupportedResult()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();

            using HttpResponseMessage response = await SendUnsupportedResultAsync(client, Guid.NewGuid());

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static string NewScenario()
        {
            string scenario = Guid.NewGuid().ToString("N");
            TestIdempotencyController.Reset(scenario);

            return scenario;
        }

        private static CustomWebApplicationFactory CreateAppWithNow(CustomWebApplicationFactory app, DateTime now)
        {
            return app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DateTimeService>();
                    services.AddSingleton(new DateTimeService(now));
                });
            });
        }

        private static Task<HttpResponseMessage> SendAsync(
            HttpClient client,
            Guid idempotencyKey,
            TestIdempotencyRequest request)
        {
            HttpRequestMessage message = new(HttpMethod.Post, _route);
            message.Headers.Add(Globals.IdempotencyKey, idempotencyKey.ToString("D"));
            message.Content = JsonContent.Create(request);

            return client.SendAsync(message);
        }

        private static Task<HttpResponseMessage> SendUnsupportedResultAsync(HttpClient client, Guid idempotencyKey)
        {
            HttpRequestMessage message = new(HttpMethod.Post, $"{_route}/unsupported-result");
            message.Headers.Add(Globals.IdempotencyKey, idempotencyKey.ToString("D"));

            return client.SendAsync(message);
        }

        private static async Task<TestIdempotencyResponse> ReadResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<TestIdempotencyResponse>()
                ?? throw new InvalidOperationException("Не удалось десериализовать тестовый ответ идемпотентности");
        }
    }
}
