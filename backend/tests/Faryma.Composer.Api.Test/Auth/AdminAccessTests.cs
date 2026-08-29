using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class AdminAccessTests : TestBase
    {
        private const string _adminProbeRoute = "/api/_test/auth/admin";

        [Fact]
        public async Task Anonymous_request_gets_401_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Browser_user_without_admin_role_gets_403_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateBrowserUserClientAsync(ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Admin_bearer_gets_200_for_admin_only_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = await app.CreateAdminBearerClientAsync(new TestAuthUserSeed
            {
                UserName = "composer_admin_access",
                Password = "TestComposerPass123!",
                Roles = [AppRoles.Composer],
            }, ct: TestContext.Current.CancellationToken);

            using HttpResponseMessage response = await client.GetAsync(_adminProbeRoute, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
