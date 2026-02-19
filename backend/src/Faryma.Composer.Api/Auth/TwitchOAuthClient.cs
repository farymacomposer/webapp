using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json.Serialization;
using Faryma.Composer.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchOAuthClient(HttpClient httpClient, IOptions<TwitchOptions> options)
    {
        private const string _tokenEndpoint = "https://id.twitch.tv/oauth2/token";
        private const string _validateEndpoint = "https://id.twitch.tv/oauth2/validate";

        public async Task<TwitchUserData> AuthenticateUser(string code, string? codeVerifier, CancellationToken cancellationToken)
        {
            string accessToken = await ExchangeCode(code, codeVerifier, cancellationToken);
            TwitchValidateResponse validation = await ValidateAccessToken(accessToken, cancellationToken);

            if (!string.Equals(validation.ClientId, options.Value.ClientId, StringComparison.Ordinal))
            {
                throw new AuthenticationException("Токен Twitch выпущен не для текущего приложения");
            }

            if (string.IsNullOrWhiteSpace(validation.UserId) || string.IsNullOrWhiteSpace(validation.Login))
            {
                throw new AuthenticationException("Twitch не вернул идентификатор пользователя");
            }

            return new TwitchUserData(validation.UserId, validation.Login);
        }

        private async Task<string> ExchangeCode(string code, string? codeVerifier, CancellationToken cancellationToken)
        {
            Dictionary<string, string> form = new()
            {
                ["client_id"] = options.Value.ClientId,
                ["client_secret"] = options.Value.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = options.Value.RedirectUri
            };

            if (!string.IsNullOrWhiteSpace(codeVerifier))
            {
                form["code_verifier"] = codeVerifier;
            }

            using HttpResponseMessage response = await httpClient.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(form), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("Не удалось обменять code на access token Twitch");
            }

            TwitchTokenResponse? token = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new AuthenticationException("Twitch не вернул access token");
            }

            return token.AccessToken;
        }

        private async Task<TwitchValidateResponse> ValidateAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new(HttpMethod.Get, _validateEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("Не удалось валидировать access token Twitch");
            }

            TwitchValidateResponse? validation = await response.Content.ReadFromJsonAsync<TwitchValidateResponse>(cancellationToken)
                ?? throw new AuthenticationException("Пустой ответ валидации Twitch");

            return validation;
        }

        private sealed record TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; init; }
        }

        private sealed record TwitchValidateResponse
        {
            [JsonPropertyName("client_id")]
            public required string ClientId { get; init; }

            [JsonPropertyName("login")]
            public required string Login { get; init; }

            [JsonPropertyName("user_id")]
            public required string UserId { get; init; }
        }
    }

    public sealed record TwitchUserData(string UserId, string Login);
}