using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchAuthService(
        TwitchOAuthClient twitchOAuthClient,
        AuthService authService,
        TwitchOAuthStateService twitchOAuthStateService,
        UserManager<UserEntity> userManager)
    {
        public async Task<string> Login(string code, string codeVerifier, string state, CancellationToken cancellationToken)
        {
            if (!twitchOAuthStateService.TryConsumeState(state))
            {
                throw new AuthenticationException("Некорректный OAuth state");
            }

            TwitchUserData twitchUser = await twitchOAuthClient.AuthenticateUser(code, codeVerifier, cancellationToken);

            UserEntity? user = await userManager.Users
                .FirstOrDefaultAsync(x => x.TwitchUserId == twitchUser.UserId, cancellationToken);

            if (user is null)
            {
                user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    UserName = $"twitch_{twitchUser.UserId}",
                    CreatedAt = DateTime.UtcNow,
                    TwitchUserId = twitchUser.UserId,
                    TwitchLogin = twitchUser.Login
                };

                IdentityResult createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    UserEntity? existingUser = await userManager.Users
                        .FirstOrDefaultAsync(x => x.TwitchUserId == twitchUser.UserId, cancellationToken);

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
            else if (!string.Equals(user.TwitchLogin, twitchUser.Login, StringComparison.Ordinal))
            {
                user.TwitchLogin = twitchUser.Login;
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

            return await authService.GenerateJwtToken(user);
        }
    }
}