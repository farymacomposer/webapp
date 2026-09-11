using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

namespace Faryma.Composer.Api.Test.Common
{
    [TestClass]
    public sealed class RequestContextMiddlewareTests : TestBase
    {
        private const string _route = "/api/_test/request-context";

        [TestMethod]
        public async Task Authenticated_request_sets_current_user_id()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync(_route, TestContext.CancellationToken);
            TestRequestContextResponse body = await ReadResponseAsync(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(body.ClaimsUserId);
            Assert.AreEqual(body.ClaimsUserId, body.UserId);
        }

        [TestMethod]
        public async Task Unauthenticated_request_leaves_current_user_id_null()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync(_route, TestContext.CancellationToken);
            TestRequestContextResponse body = await ReadResponseAsync(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(body.UserId);
            Assert.IsNull(body.ClaimsUserId);
            Assert.AreNotEqual(default, body.Now);
        }

        private async Task<TestRequestContextResponse> ReadResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<TestRequestContextResponse>(TestContext.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать тестовый ответ request-context");
        }
    }
}
