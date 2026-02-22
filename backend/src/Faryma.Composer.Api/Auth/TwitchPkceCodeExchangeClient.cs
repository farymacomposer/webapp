using System.Security.Authentication;
using System.Text.Json.Serialization;
using Faryma.Composer.Contracts.Api.Auth.Contracts;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchPkceCodeExchangeClient(
        HttpClient httpClient,
        IOptions<TwitchOptions> options) : ITwitchPkceCodeExchangeClient
    {
        private const string _tokenEndpoint = "https://id.twitch.tv/oauth2/token";

        public async Task<string> ExchangeCodeWithPkce(string code, string codeVerifier, CancellationToken ct)
        {
            Dictionary<string, string> form = new()
            {
                ["client_id"] = options.Value.ClientId,
                ["client_secret"] = options.Value.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = options.Value.RedirectUri,
                ["code_verifier"] = codeVerifier
            };

            using HttpResponseMessage response = await httpClient.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(form), ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("Не удалось обменять code на access token Twitch");
            }

            TwitchTokenResponse? token = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(ct);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new AuthenticationException("Twitch не вернул access token");
            }

            return token.AccessToken;
        }

        private sealed record TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; init; }
        }
    }
}