using System.Security.Authentication;
using System.Security.Claims;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Api.Features.Auth.Services
{
    public sealed class TwitchAuthService(
        UserManager<UserEntity> userManager,
        DateTimeService dateTimeService)
    {
        public async Task<ClaimsPrincipal> CreateBrowserPrincipal(ClaimsPrincipal twitchPrincipal, CancellationToken ct)
        {
            string twitchUserId = twitchPrincipal.FindFirstValue("sub")
                ?? throw new AuthenticationException("Twitch не вернул идентификатор пользователя");
            string? twitchLogin = twitchPrincipal.FindFirstValue("preferred_username");

            UserEntity? user = await userManager.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId, ct);
            if (user is null)
            {
                user = await Create(twitchUserId, twitchLogin, ct);
            }
            else if (!string.IsNullOrWhiteSpace(twitchLogin) && !string.Equals(user.TwitchLogin, twitchLogin, StringComparison.Ordinal))
            {
                await UpdateTwitchLogin(user, twitchLogin);
            }

            return await BuildPrincipal(user);
        }

        private static string GetErrors(IdentityResult identityResult) => string.Join("; ", identityResult.Errors.Select(x => x.Description));

        private async Task<UserEntity> Create(string twitchUserId, string? twitchLogin, CancellationToken ct)
        {
            UserEntity result = new()
            {
                Id = Guid.NewGuid(),
                CreatedAt = dateTimeService.Now,
                TwitchUserId = twitchUserId,
                TwitchLogin = twitchLogin
            };

            IdentityResult createResult = await userManager.CreateAsync(result);
            if (!createResult.Succeeded)
            {
                UserEntity? existingUser = await userManager.Users.FirstOrDefaultAsync(x => x.TwitchUserId == twitchUserId, ct)
                    ?? throw new AuthenticationException($"Не удалось создать пользователя Twitch: {GetErrors(createResult)}");

                result = existingUser;
            }

            return result;
        }

        private async Task UpdateTwitchLogin(UserEntity user, string? twitchLogin)
        {
            user.TwitchLogin = twitchLogin;
            IdentityResult updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new AuthenticationException($"Не удалось обновить Twitch-логин пользователя: {GetErrors(updateResult)}");
            }
        }

        private async Task<ClaimsPrincipal> BuildPrincipal(UserEntity user)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.TwitchLogin ?? user.UserName ?? user.Id.ToString()),
            ];

            if (!string.IsNullOrWhiteSpace(user.TwitchUserId))
            {
                claims.Add(new Claim("twitch_user_id", user.TwitchUserId));
            }

            if (!string.IsNullOrWhiteSpace(user.TwitchLogin))
            {
                claims.Add(new Claim("preferred_username", user.TwitchLogin));
            }

            IList<string> roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            ClaimsIdentity identity = new(claims, AppAuthenticationSchemes.BrowserCookieScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
