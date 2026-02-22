using System.Security.Authentication;
using Faryma.Composer.Contracts.Api.Auth.Contracts;
using Faryma.Composer.Contracts.Api.Auth.Models;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchAuthClient(
        ITwitchTokenValidationClient twitchTokenValidationClient,
        ITwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient,
        IOptions<TwitchOptions> options)
    {
        public async Task<TwitchUserData> AuthenticateUser(string code, string codeVerifier, CancellationToken ct)
        {
            ValidateInput(code, codeVerifier);

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

        private static void ValidateInput(string code, string codeVerifier)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new AuthenticationException("Параметр code обязателен");
            }

            if (string.IsNullOrWhiteSpace(codeVerifier))
            {
                throw new AuthenticationException("Параметр code_verifier обязателен");
            }

            if (codeVerifier.Length is < 43 or > 128)
            {
                throw new AuthenticationException("Некорректная длина code_verifier");
            }

            if (codeVerifier.Any(static symbol => !IsPkceUnreserved(symbol)))
            {
                throw new AuthenticationException("code_verifier содержит недопустимые символы");
            }
        }

        private static bool IsPkceUnreserved(char symbol) => char.IsAsciiLetterOrDigit(symbol) || symbol is '-' or '.' or '_' or '~';

        private async Task<string> ExchangeCode(string code, string codeVerifier, CancellationToken ct)
        {
            try
            {
                return await twitchPkceCodeExchangeClient.ExchangeCodeWithPkce(code, codeVerifier, ct);
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
                return await twitchTokenValidationClient.ValidateAccessToken(accessToken, ct);
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
    }
}