using System.Security.Authentication;
using Faryma.Composer.Api.Auth.Options;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchOAuthClient(
        ITwitchTokenValidationClient twitchTokenValidationClient,
        ITwitchPkceCodeExchangeClient twitchPkceCodeExchangeClient,
        IOptions<TwitchOptions> options)
    {
        public async Task<TwitchUserData> AuthenticateUser(string code, string codeVerifier, CancellationToken cancellationToken)
        {
            ValidateInput(code, codeVerifier);

            string accessToken = await ExchangeCode(code, codeVerifier, cancellationToken);
            TwitchValidateData validation = await ValidateAccessToken(accessToken, cancellationToken);

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

        private async Task<string> ExchangeCode(string code, string codeVerifier, CancellationToken cancellationToken)
        {
            try
            {
                return await twitchPkceCodeExchangeClient.ExchangeCodeWithPkce(code, codeVerifier, cancellationToken);
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

        private async Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                return await twitchTokenValidationClient.ValidateAccessToken(accessToken, cancellationToken);
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

        private static bool IsPkceUnreserved(char symbol) =>
            char.IsAsciiLetterOrDigit(symbol) || symbol is '-' or '.' or '_' or '~';
    }

    public sealed record TwitchUserData(string UserId, string Login);
}