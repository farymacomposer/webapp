using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class AuthHelpersTests : TestBase
    {
        [Fact]
        public async Task Anonymous_client_gets_401_for_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/authenticated", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Admin_bearer_client_can_access_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateAdminBearerClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_without_admin_role_is_authenticated_without_twitch_callback()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/authenticated", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_without_admin_role_gets_403_for_admin_probe()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_with_admin_role_can_access_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(new TestAuthUserSeed
            {
                UserName = "browser_admin_user",
                TwitchUserId = "browser-admin-user-id",
                TwitchLogin = "browser_admin_user",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
