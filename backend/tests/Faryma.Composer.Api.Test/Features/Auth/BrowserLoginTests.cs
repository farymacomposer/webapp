using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Faryma.Composer.Api.Test.Features.Auth
{
    [TestClass]
    public sealed class BrowserLoginTests : TestBase
    {
        [TestMethod]
        public async Task Browser_login_challenges_twitch_oidc_authorize_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = CreateBrowserLoginClient(app);

            using HttpResponseMessage response = await client.GetAsync("/api/Auth/TwitchLogin", TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
            Assert.IsNotNull(response.Headers.Location);

            Uri location = response.Headers.Location!;
            Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(location.Query);

            Assert.AreEqual("https", location.Scheme);
            Assert.AreEqual("id.twitch.tv", location.Host);
            Assert.AreEqual("/oauth2/authorize", location.AbsolutePath);
            Assert.AreEqual("test-twitch-client-id-1234567890", query["client_id"].ToString());
            Assert.AreEqual("https://localhost/signin-oidc", query["redirect_uri"].ToString());
            Assert.AreEqual("code", query["response_type"].ToString());
            Assert.Contains("openid", query["scope"].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static HttpClient CreateBrowserLoginClient(CustomWebApplicationFactory app)
        {
            CustomWebApplicationFactory browserLoginApp = app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.PostConfigureAll<OpenIdConnectOptions>(options =>
                    {
                        options.Configuration = new OpenIdConnectConfiguration
                        {
                            AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize",
                            Issuer = "https://id.twitch.tv/oauth2",
                        };
                    });
                });
            });

            return browserLoginApp.CreateAnonymousClient();
        }
    }
}
