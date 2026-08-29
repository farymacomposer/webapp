using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class BrowserLoginTests : TestBase
    {
        [Fact]
        public async Task Browser_login_challenges_twitch_oidc_authorize_endpoint()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = CreateBrowserLoginClient(app);

            using HttpResponseMessage response = await client.GetAsync("/api/auth/oauth/twitch", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            Uri location = response.Headers.Location!;
            Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(location.Query);

            Assert.Equal("https", location.Scheme);
            Assert.Equal("id.twitch.tv", location.Host);
            Assert.Equal("/oauth2/authorize", location.AbsolutePath);
            Assert.Equal("test-twitch-client-id-1234567890", query["client_id"].ToString());
            Assert.Equal("https://localhost/signin-oidc", query["redirect_uri"].ToString());
            Assert.Equal("code", query["response_type"].ToString());
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
