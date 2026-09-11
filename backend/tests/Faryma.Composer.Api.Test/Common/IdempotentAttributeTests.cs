using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Common
{
    [TestClass]
    [DoNotParallelize]
    public sealed class IdempotentAttributeTests : DatabaseTestBase
    {
        private const string _route = "/api/_test/idempotency";

        [TestMethod]
        public async Task IdempotentEndpoint_ReplaysStoredResponse_ForSameIdempotencyKey()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "same-payload");

            using HttpResponseMessage firstResponse = await SendAsync(client, idempotencyKey, request);
            using HttpResponseMessage secondResponse = await SendAsync(client, idempotencyKey, request);

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse second = await ReadResponseAsync(secondResponse);

            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.AreEqual(first, second);
            Assert.AreEqual(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [TestMethod]
        public async Task IdempotentEndpoint_ExecutesOnce_ForConcurrentSameIdempotencyKey()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "same-payload", DelayMilliseconds: 200);

            Task<HttpResponseMessage> firstTask = SendAsync(client, idempotencyKey, request);
            Task<HttpResponseMessage> secondTask = SendAsync(client, idempotencyKey, request);
            HttpResponseMessage[] responses = await Task.WhenAll(firstTask, secondTask);

            using HttpResponseMessage firstResponse = responses[0];
            using HttpResponseMessage secondResponse = responses[1];

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse second = await ReadResponseAsync(secondResponse);

            foreach (HttpResponseMessage response in responses)
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            Assert.AreEqual(first, second);
            Assert.AreEqual(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [TestMethod]
        public async Task IdempotentEndpoint_ReturnsConflict_WhenSameKeyUsesDifferentPayload()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            Guid idempotencyKey = Guid.NewGuid();

            using HttpResponseMessage firstResponse = await SendAsync(client, idempotencyKey, new TestIdempotencyRequest(scenario, "first"));
            using HttpResponseMessage secondResponse = await SendAsync(client, idempotencyKey, new TestIdempotencyRequest(scenario, "second"));

            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.AreEqual(HttpStatusCode.Conflict, secondResponse.StatusCode);
            Assert.AreEqual(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [TestMethod]
        public async Task IdempotentEndpoint_DoesNotStoreIdempotencyRecord_WhenActionFails()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            Guid idempotencyKey = Guid.NewGuid();
            TestIdempotencyRequest request = new(scenario, "retry-after-error");
            TestIdempotencyController.FailNext(scenario);

            using HttpResponseMessage failedResponse = await SendAsync(client, idempotencyKey, request);
            using HttpResponseMessage retryResponse = await SendAsync(client, idempotencyKey, request);

            TestIdempotencyResponse retry = await ReadResponseAsync(retryResponse);

            Assert.AreEqual(HttpStatusCode.Conflict, failedResponse.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, retryResponse.StatusCode);
            Assert.AreEqual(new TestIdempotencyResponse(scenario, "retry-after-error", 1), retry);
            Assert.AreEqual(1, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [TestMethod]
        public async Task IdempotentEndpoint_ExecutesAgain_WhenIdempotencyKeyExpired()
        {
            string scenario = NewScenario();
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            Guid idempotencyKey = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            TestIdempotencyRequest request = new(scenario, "expired-key");

            CustomWebApplicationFactory firstApp = CreateAppWithNow(app, now);
            using HttpClient firstClient = await firstApp.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            using HttpResponseMessage firstResponse = await SendAsync(firstClient, idempotencyKey, request);

            CustomWebApplicationFactory laterApp = CreateAppWithNow(app, now.AddHours(2));
            using HttpClient laterClient = await laterApp.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);
            using HttpResponseMessage laterResponse = await SendAsync(laterClient, idempotencyKey, request);

            TestIdempotencyResponse first = await ReadResponseAsync(firstResponse);
            TestIdempotencyResponse later = await ReadResponseAsync(laterResponse);

            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, laterResponse.StatusCode);
            Assert.AreEqual(1, first.Executions);
            Assert.AreEqual(2, later.Executions);
            Assert.AreEqual(2, TestIdempotencyController.GetExecutionCount(scenario));
        }

        [TestMethod]
        public async Task IdempotentEndpoint_Throws_WhenActionReturnsUnsupportedResult()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);

            using HttpResponseMessage response = await SendUnsupportedResultAsync(client, Guid.NewGuid());

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private static string NewScenario()
        {
            string scenario = Guid.NewGuid().ToString("N");
            TestIdempotencyController.Reset(scenario);

            return scenario;
        }

        private static CustomWebApplicationFactory CreateAppWithNow(CustomWebApplicationFactory app, DateTime now) =>
            app.WithFixedDateTime(now);

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
