using System.Net;
using System.Security.Authentication;
using System.Text;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Test
{
    public sealed class TwitchPkceCodeExchangeClientTest
    {
        private sealed class RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }

            public string LastBody { get; private set; } = string.Empty;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                if (request.Content is not null)
                {
                    LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
                }

                return responseFactory(request);
            }
        }

        [Fact]
        public async Task ExchangeCodeWithPkce_ReturnsToken_WhenResponseIsValid()
        {
            RecordingHttpMessageHandler handler = new(_ =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"pkce-token\"}", Encoding.UTF8, "application/json")
                };
            });
            TwitchPkceCodeExchangeClient sut = CreateSut(handler);

            string token = await sut.ExchangeCodeWithPkce("oauth-code", "verifier", CancellationToken.None);

            Assert.Equal("pkce-token", token);
            Assert.NotNull(handler.LastRequest);
            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Equal("https://id.twitch.tv/oauth2/token", handler.LastRequest.RequestUri!.ToString());
            Assert.Contains("code=oauth-code", handler.LastBody, StringComparison.Ordinal);
            Assert.Contains("code_verifier=verifier", handler.LastBody, StringComparison.Ordinal);
            Assert.Contains("grant_type=authorization_code", handler.LastBody, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExchangeCodeWithPkce_Throws_WhenHttpStatusIsNotSuccess()
        {
            RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
            TwitchPkceCodeExchangeClient sut = CreateSut(handler);

            await Assert.ThrowsAsync<AuthenticationException>(
                () => sut.ExchangeCodeWithPkce("oauth-code", "verifier", CancellationToken.None));
        }

        [Fact]
        public async Task ExchangeCodeWithPkce_Throws_WhenAccessTokenIsMissing()
        {
            RecordingHttpMessageHandler handler = new(_ =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"\"}", Encoding.UTF8, "application/json")
                };
            });
            TwitchPkceCodeExchangeClient sut = CreateSut(handler);

            await Assert.ThrowsAsync<AuthenticationException>(
                () => sut.ExchangeCodeWithPkce("oauth-code", "verifier", CancellationToken.None));
        }

        private static TwitchPkceCodeExchangeClient CreateSut(RecordingHttpMessageHandler handler)
        {
            HttpClient httpClient = new(handler);
            IOptions<TwitchOptions> options = Options.Create(new TwitchOptions
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                RedirectUri = "https://example.com/auth/twitch/callback"
            });

            return new TwitchPkceCodeExchangeClient(httpClient, options);
        }
    }
}