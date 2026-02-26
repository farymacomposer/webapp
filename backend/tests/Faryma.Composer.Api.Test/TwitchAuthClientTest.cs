using System.Security.Authentication;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Contracts.Api.Auth.Contracts;
using Faryma.Composer.Contracts.Api.Auth.Models;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Test
{
    public sealed class TwitchAuthClientTest
    {
        private sealed class FakeTwitchTokenValidationClient : ITwitchTokenValidationClient
        {
            public string? ValidatedAccessToken { get; private set; }

            public TwitchValidateData ValidationResponse { get; set; } = new("", "", "");

            public Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken ct)
            {
                ValidatedAccessToken = accessToken;
                return Task.FromResult(ValidationResponse);
            }
        }

        private sealed class FakeTwitchPkceCodeExchangeClient : ITwitchPkceCodeExchangeClient
        {
            public bool ExchangeCodeWithPkceCalled { get; private set; }

            public string? Code { get; private set; }

            public string? CodeVerifier { get; private set; }

            public string AccessTokenToReturn { get; set; } = "pkce-access-token";

            public Task<string> ExchangeCodeWithPkce(string code, string codeVerifier, CancellationToken ct)
            {
                ExchangeCodeWithPkceCalled = true;
                Code = code;
                CodeVerifier = codeVerifier;

                return Task.FromResult(AccessTokenToReturn);
            }
        }

        [Fact]
        public async Task AuthenticateUser_Throws_WhenCodeVerifierIsTooShort()
        {
            FakeTwitchTokenValidationClient twitchTokenValidationClient = new();
            FakeTwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient = new();

            TwitchAuthClient sut = CreateSut(twitchTokenValidationClient, twitchPkceCodeExchangeClient);

            await Assert.ThrowsAsync<AuthenticationException>(() => sut.AuthenticateUser("oauth-code", "short-verifier", CancellationToken.None));
            Assert.False(twitchPkceCodeExchangeClient.ExchangeCodeWithPkceCalled);
        }

        [Fact]
        public async Task AuthenticateUser_WithCodeVerifier_UsesPkceExchangeClient()
        {
            FakeTwitchTokenValidationClient twitchTokenValidationClient = new()
            {
                ValidationResponse = new TwitchValidateData("client-id", "pkce_login", "pkce-user")
            };

            FakeTwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient = new()
            {
                AccessTokenToReturn = "token_pkce"
            };

            string codeVerifier = new('a', 43);
            TwitchAuthClient sut = CreateSut(twitchTokenValidationClient, twitchPkceCodeExchangeClient);
            TwitchUserData result = await sut.AuthenticateUser("oauth-code", codeVerifier, CancellationToken.None);

            Assert.Equal("pkce_login", result.Login);
            Assert.Equal("pkce-user", result.UserId);
            Assert.True(twitchPkceCodeExchangeClient.ExchangeCodeWithPkceCalled);
            Assert.Equal("oauth-code", twitchPkceCodeExchangeClient.Code);
            Assert.Equal(codeVerifier, twitchPkceCodeExchangeClient.CodeVerifier);
            Assert.Equal("token_pkce", twitchTokenValidationClient.ValidatedAccessToken);
        }

        [Fact]
        public async Task AuthenticateUser_Throws_WhenTokenIssuedForAnotherClient()
        {
            FakeTwitchTokenValidationClient twitchTokenValidationClient = new()
            {
                ValidationResponse = new TwitchValidateData("another-client-id", "streamer_login", "user-1")
            };

            FakeTwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient = new()
            {
                AccessTokenToReturn = "token_pkce"
            };

            string codeVerifier = new('b', 43);
            TwitchAuthClient sut = CreateSut(twitchTokenValidationClient, twitchPkceCodeExchangeClient);

            await Assert.ThrowsAsync<AuthenticationException>(() => sut.AuthenticateUser("oauth-code", codeVerifier, CancellationToken.None));
        }

        private static TwitchAuthClient CreateSut(
            ITwitchTokenValidationClient twitchTokenValidationClient,
            ITwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient)
        {
            IOptions<TwitchOptions> options = Options.Create(new TwitchOptions
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                RedirectUri = "https://example.com/auth/twitch/callback"
            });

            return new TwitchAuthClient(twitchTokenValidationClient, twitchPkceCodeExchangeClient, options);
        }
    }
}