using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

namespace Faryma.Composer.Api.Test.Common
{
    public sealed class RequestContextMiddlewareTests : TestBase
    {
        private const string _route = "/api/_test/request-context";

        [Fact]
        public async Task Authenticated_request_sets_current_user_id()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync(_route, TestContext.Current.CancellationToken);
            TestRequestContextResponse body = await ReadResponseAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body.ClaimsUserId);
            Assert.Equal(body.ClaimsUserId, body.UserId);
        }

        [Fact]
        public async Task Unauthenticated_request_leaves_current_user_id_null()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync(_route, TestContext.Current.CancellationToken);
            TestRequestContextResponse body = await ReadResponseAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(body.UserId);
            Assert.Null(body.ClaimsUserId);
            Assert.NotEqual(default, body.Now);
        }

        private static async Task<TestRequestContextResponse> ReadResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<TestRequestContextResponse>(TestContext.Current.CancellationToken)
                ?? throw new InvalidOperationException("Не удалось десериализовать тестовый ответ request-context");
        }
    }
}
