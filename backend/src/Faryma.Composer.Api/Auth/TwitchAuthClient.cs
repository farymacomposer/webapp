using System.Security.Authentication;
using System.Text.Json.Serialization;
using Faryma.Composer.Contracts.Api.Auth.Contracts;
using Faryma.Composer.Contracts.Api.Auth.Models;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
using TwitchLib.Api.Auth;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchAuthClient(HttpClient httpClient, IOptions<TwitchOptions> options)
    {
        private const string _tokenEndpoint = "https://id.twitch.tv/oauth2/token";

        public async Task<TwitchUserData> AuthenticateUser(string code, string codeVerifier, CancellationToken ct)
        {
            string accessToken = await ExchangeCode(code, codeVerifier, ct);
            TwitchValidateData validation = await ValidateAccessToken(accessToken, ct);

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

        private async Task<string> ExchangeCode(string code, string codeVerifier, CancellationToken ct)
        {
            try
            {
                return await ExchangeCodeWithPkce(code, codeVerifier, ct);
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Не удалось обменять code на access token Twitch", exception);
            }
        }

        private async Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken ct)
        {
            try
            {
                TwitchAPI twitchApi = new();
                twitchApi.Settings.ClientId = options.Value.ClientId;

                // TODO: Разобраться с WaitAsync
                ValidateAccessTokenResponse? validation = await twitchApi.Auth.ValidateAccessTokenAsync(accessToken).WaitAsync(ct)
                    ?? throw new AuthenticationException("Пустой ответ валидации Twitch");

                return new TwitchValidateData(validation.ClientId, validation.Login, validation.UserId);
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Не удалось валидировать access token Twitch", exception);
            }
        }

        private sealed record TwitchTokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; init; }
        }
    }
}