using System.Security.Authentication;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api.Auth;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class TwitchAuthService(
        TwitchAuthClient twitchAuthClient,
        AuthTokenService authTokenService,
        TwitchAuthStateService twitchAuthStateService,
        UserManager<UserEntity> userManager,
        DateTimeService dateTimeService)
    {
        public async Task<(string AccessToken, string RefreshToken)> Login(
            string code,
            string codeVerifier,
            string state,
            string? browserNonce,
            CancellationToken ct)
        {
            if (!twitchAuthStateService.TryConsumeState(state, browserNonce))
            {
                throw new AuthenticationException("Некорректный OAuth state");
            }

            ValidateAccessTokenResponse twitchToken = await twitchAuthClient.AuthenticateUser(code, codeVerifier, ct);
            UserEntity? user = await userManager.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchToken.UserId, ct);

            if (user is null)
            {
                user = await Create(twitchToken, ct);
            }
            else if (!string.Equals(user.TwitchLogin, twitchToken.Login, StringComparison.Ordinal))
            {
                await UpdateTwitchLogin(user, twitchToken.Login);
            }

            return await authTokenService.IssueForUser(user, ct);
        }

        private static string GetErrors(IdentityResult identityResult) => string.Join("; ", identityResult.Errors.Select(x => x.Description));

        private async Task<UserEntity> Create(ValidateAccessTokenResponse twitchToken, CancellationToken ct)
        {
            UserEntity result = new()
            {
                Id = Guid.NewGuid(),
                CreatedAt = dateTimeService.Now,
                TwitchUserId = twitchToken.UserId,
                TwitchLogin = twitchToken.Login
            };

            IdentityResult createResult = await userManager.CreateAsync(result);
            if (!createResult.Succeeded)
            {
                UserEntity? existingUser = await userManager.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchToken.UserId, ct)
                    ?? throw new AuthenticationException($"Не удалось создать пользователя Twitch: {GetErrors(createResult)}");

                result = existingUser;
            }

            return result;
        }

        private async Task UpdateTwitchLogin(UserEntity user, string twitchLogin)
        {
            user.TwitchLogin = twitchLogin;
            IdentityResult updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new AuthenticationException($"Не удалось обновить Twitch-логин пользователя: {GetErrors(updateResult)}");
            }
        }
    }
}