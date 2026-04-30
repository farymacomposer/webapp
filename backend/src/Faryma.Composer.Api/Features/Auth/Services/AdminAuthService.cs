using System.Security.Claims;
using Faryma.Composer.Contracts.Api.Features.Auth.Login;
using Faryma.Composer.Contracts.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Api.Features.Auth.Services
{
    public sealed class AdminAuthService(UserManager<UserEntity> userManager)
    {
        public async Task<AuthenticatedAdmin?> Authenticate(LoginRequest request, CancellationToken ct)
        {
            string userName = request.UserName.Trim();
            UserEntity? user = await userManager.FindByNameAsync(userName);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                await DelayFailedAuthentication(ct);

                return null;
            }

            IList<string> roles = await userManager.GetRolesAsync(user);
            if (!roles.Any(IsAdminRole))
            {
                await DelayFailedAuthentication(ct);

                return null;
            }

            return new AuthenticatedAdmin(user, roles.ToArray());
        }

        public ClaimsPrincipal CreateBrowserPrincipal(AuthenticatedAdmin admin)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, admin.User.Id.ToString()),
                new(ClaimTypes.Name, admin.User.UserName ?? admin.User.Id.ToString()),
            ];

            claims.AddRange(admin.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            ClaimsIdentity identity = new(claims, AppAuthenticationSchemes.BrowserCookieScheme);

            return new ClaimsPrincipal(identity);
        }

        private static Task DelayFailedAuthentication(CancellationToken ct) => Task.Delay(1000, ct);

        private static bool IsAdminRole(string role)
        {
            return string.Equals(role, AppRoles.Composer, StringComparison.Ordinal)
                || string.Equals(role, AppRoles.Moderator, StringComparison.Ordinal);
        }
    }

    public sealed record AuthenticatedAdmin(UserEntity User, IReadOnlyCollection<string> Roles);
}
