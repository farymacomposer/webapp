using System.Security.Authentication;
using Faryma.Composer.Contracts.Api.Auth.Contracts;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Microsoft.Extensions.Options;
using TwitchLib.Api;
using TwitchLib.Api.Auth;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchTokenValidationClient(IOptions<TwitchOptions> options) : ITwitchTokenValidationClient
    {
        public async Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken ct)
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
    }
}