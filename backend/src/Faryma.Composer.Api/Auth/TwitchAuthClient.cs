using System.Security.Authentication;
using System.Text.Json.Serialization;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
using TwitchLib.Api.Auth;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchAuthClient(HttpClient httpClient, IOptions<TwitchOptions> options)
    {
        private const string _tokenEndpoint = "https://id.twitch.tv/oauth2/token";

        public async Task<ValidateAccessTokenResponse> AuthenticateUser(string code, string codeVerifier, CancellationToken ct)
        {
            string accessToken = await ExchangeCode(code, codeVerifier, ct);
            ValidateAccessTokenResponse result = await ValidateAccessToken(accessToken, ct);

            if (!string.Equals(result.ClientId, options.Value.ClientId, StringComparison.Ordinal))
            {
                throw new AuthenticationException("Токен Twitch выпущен не для текущего приложения");
            }

            if (string.IsNullOrWhiteSpace(result.UserId) || string.IsNullOrWhiteSpace(result.Login))
            {
                throw new AuthenticationException("Twitch не вернул идентификатор пользователя");
            }

            return result;
        }

        private async Task<string> ExchangeCode(string code, string codeVerifier, CancellationToken ct)
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
            if (string.IsNullOrWhiteSpace(token?.AccessToken))
            {
                throw new AuthenticationException("Twitch не вернул access token");
            }

            return token.AccessToken;
        }

        private async Task<ValidateAccessTokenResponse> ValidateAccessToken(string accessToken, CancellationToken ct)
        {
            TwitchAPI twitchApi = new();
            twitchApi.Settings.ClientId = options.Value.ClientId;

            // TODO: Разобраться с WaitAsync
            ValidateAccessTokenResponse? result = await twitchApi.Auth.ValidateAccessTokenAsync(accessToken).WaitAsync(ct)
                ?? throw new AuthenticationException("Пустой ответ валидации Twitch");

            return result;
        }

        private sealed record TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; init; }
        }
    }
}
