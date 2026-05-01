using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class AdminAccessTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        private const string _adminProbeRoute = "/api/_test/auth/admin";

        [Fact]
        public async Task Anonymous_request_gets_401_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_admin_role_gets_403_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateBrowserUserClientAsync();

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Admin_bearer_gets_200_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = await app.CreateAdminBearerClientAsync(new AdminBearerClientOptions
            {
                User = new TestAuthUserSeed
                {
                    UserName = "composer_admin_access",
                    Password = "TestComposerPass123!",
                    Roles = [TestAuthRoles.Admin],
                },
            });

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
