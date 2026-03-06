using System.Security.Authentication;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api.Auth;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class TwitchAuthService(
        TwitchAuthClient twitchAuthClient,
        AuthTokenService authTokenService,
        TwitchAuthStateService twitchAuthStateService,
        UserManager<UserEntity> userManager)
    {
        public async Task<(string AccessToken, string RefreshToken)> Login(
            string code,
            string codeVerifier,
            string state,
            string? browserNonce,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (!twitchAuthStateService.TryConsumeState(state, browserNonce))
            {
                throw new AuthenticationException("Некорректный OAuth state");
            }

            ValidateAccessTokenResponse twitchToken = await twitchAuthClient.AuthenticateUser(code, codeVerifier, cancellationToken);

            UserEntity? user = await userManager.Users
                .FirstOrDefaultAsync(x => x.TwitchUserId == twitchToken.UserId, cancellationToken);

            if (user is null)
            {
                user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    UserName = $"twitch_{twitchToken.UserId}",
                    CreatedAt = now,
                    TwitchUserId = twitchToken.UserId,
                    TwitchLogin = twitchToken.Login
                };

                IdentityResult createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    UserEntity? existingUser = await userManager.Users
                        .FirstOrDefaultAsync(x => x.TwitchUserId == twitchToken.UserId, cancellationToken);

                    if (existingUser is not null)
                    {
                        user = existingUser;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Не удалось создать пользователя Twitch: {string.Join("; ", createResult.Errors.Select(x => x.Description))}");
                    }
                }
            }
            else if (!string.Equals(user.TwitchLogin, twitchToken.Login, StringComparison.Ordinal))
            {
                user.TwitchLogin = twitchToken.Login;
                IdentityResult updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException($"Не удалось обновить Twitch-логин пользователя: {string.Join("; ", updateResult.Errors.Select(x => x.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, "User"))
            {
                IdentityResult addRoleResult = await userManager.AddToRoleAsync(user, "User");
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Не удалось назначить роль User: {string.Join("; ", addRoleResult.Errors.Select(x => x.Description))}");
                }
            }

            return await authTokenService.IssueForUser(user, now, cancellationToken);
        }
    }
}