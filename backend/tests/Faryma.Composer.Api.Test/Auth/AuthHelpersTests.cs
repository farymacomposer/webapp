using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class AuthHelpersTests(PostgreSqlFixture fixture) : ApiTestBase(fixture)
    {
        [Fact]
        public async Task Anonymous_client_gets_401_for_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/authenticated");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Admin_bearer_client_can_access_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_without_admin_role_is_authenticated_without_twitch_callback()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/authenticated");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_without_admin_role_gets_403_for_admin_probe()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_client_with_admin_role_can_access_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync(new BrowserUserClientOptions
            {
                User = new TestAuthUserSeed
                {
                    UserName = "browser_admin_user",
                    TwitchUserId = "browser-admin-user-id",
                    TwitchLogin = "browser_admin_user",
                    Roles = [TestAuthRoles.Admin],
                },
            });

            using HttpResponseMessage response = await client.GetAsync("/api/_test/auth/admin");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
