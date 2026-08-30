using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Faryma.Composer.Api.Test.Infrastructure.Auth
{
    public static class CustomWebApplicationFactoryAuthExtensions
    {
        public static async Task<SeededAuthUsers> SeedUsersAsync(
            this CustomWebApplicationFactory app,
            AuthTestSeedOptions? options = null,
            CancellationToken ct = default)
        {
            options ??= new AuthTestSeedOptions();

            SeededAuthUser admin = await app.EnsureUserAsync(options.Admin, ct);
            SeededAuthUser browser = await app.EnsureUserAsync(options.Browser, ct);

            return new SeededAuthUsers(admin, browser);
        }

        public static async Task<HttpClient> CreateAdminBearerClientAsync(
            this CustomWebApplicationFactory app,
            TestAuthUserSeed? seed = null,
            CancellationToken ct = default)
        {
            seed ??= new AuthTestSeedOptions().Admin;

            if (!app.UsesDatabase)
            {
                HttpClient noDatabaseClient = app.CreateAnonymousClient();
                noDatabaseClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    CreateAccessToken(seed));

                return noDatabaseClient;
            }

            SeededAuthUser admin = await app.EnsureUserAsync(seed, ct);
            if (string.IsNullOrWhiteSpace(admin.Password))
            {
                throw new InvalidOperationException("Для вспомогательного метода создания bearer-клиента администратора требуется тестовый пароль");
            }

            (string accessToken, _) = await app.Services.RunInScopeAsync(async scoped =>
            {
                AuthTokenService authTokenService = scoped.GetRequiredService<AuthTokenService>();
                UserManager<UserEntity> userManager = scoped.GetRequiredService<UserManager<UserEntity>>();
                UserEntity user = await userManager.FindByIdAsync(admin.UserId.ToString())
                    ?? throw new InvalidOperationException($"Не удалось загрузить тестового администратора '{admin.UserName}'");

                return await authTokenService.IssueForUser(user, ct);
            });

            HttpClient client = app.CreateAnonymousClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        public static async Task<HttpClient> CreateBrowserUserClientAsync(
            this CustomWebApplicationFactory app,
            TestAuthUserSeed? seed = null,
            CancellationToken ct = default)
        {
            seed ??= new AuthTestSeedOptions().Browser;

            BrowserUserAuthenticationStateHolder stateHolder = new();

            CustomWebApplicationFactory browserApp = app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(stateHolder);
                    services
                        .AddAuthentication(authenticationOptions =>
                        {
                            authenticationOptions.DefaultScheme = BrowserUserTestAuthHandler.SchemeName;
                            authenticationOptions.DefaultAuthenticateScheme = BrowserUserTestAuthHandler.SchemeName;
                            authenticationOptions.DefaultChallengeScheme = BrowserUserTestAuthHandler.SchemeName;
                        })
                        .AddScheme<AuthenticationSchemeOptions, BrowserUserTestAuthHandler>(BrowserUserTestAuthHandler.SchemeName, _ => { });
                });
            });

            HttpClient client = browserApp.CreateAnonymousClient();
            SeededAuthUser seededUser = browserApp.UsesDatabase
                ? await browserApp.EnsureUserAsync(seed, ct)
                : CreateSeededUser(seed);

            stateHolder.State = new BrowserUserAuthenticationState(
                seededUser.UserId,
                seededUser.UserName,
                seededUser.TwitchUserId,
                seededUser.TwitchLogin,
                seededUser.Roles);

            return client;
        }

        private static string CreateAccessToken(TestAuthUserSeed seed)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, seed.TwitchLogin ?? seed.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ];

            claims.AddRange(seed.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(TestConfiguration.JwtSecretKey));
            JwtSecurityToken token = new(
                issuer: TestConfiguration.JwtIssuer,
                audience: TestConfiguration.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static SeededAuthUser CreateSeededUser(TestAuthUserSeed seed)
        {
            return new SeededAuthUser(
                Guid.NewGuid(),
                seed.UserName,
                seed.Password,
                seed.TwitchUserId,
                seed.TwitchLogin,
                seed.Roles);
        }

        private static Task<SeededAuthUser> EnsureUserAsync(
            this CustomWebApplicationFactory app,
            TestAuthUserSeed seed,
            CancellationToken ct) =>
            app.Services.RunInScopeAsync(async scoped =>
            {
                UserManager<UserEntity> userManager = scoped.GetRequiredService<UserManager<UserEntity>>();
                RoleManager<IdentityRole<Guid>> roleManager = scoped.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                UserEntity? user = await userManager.FindByNameAsync(seed.UserName);
                if (user is null && !string.IsNullOrWhiteSpace(seed.TwitchUserId))
                {
                    user = await userManager.Users.FirstOrDefaultAsync(x => x.TwitchUserId == seed.TwitchUserId, ct);
                }

                if (user is null)
                {
                    user = new UserEntity
                    {
                        Id = Guid.NewGuid(),
                        UserName = seed.UserName,
                        NormalizedUserName = userManager.NormalizeName(seed.UserName),
                        TwitchUserId = seed.TwitchUserId,
                        TwitchLogin = seed.TwitchLogin,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await EnsureSuccess(await userManager.CreateAsync(user), $"Не удалось создать тестового пользователя '{seed.UserName}'");
                }
                else
                {
                    var changed = false;

                    if (!string.Equals(user.UserName, seed.UserName, StringComparison.Ordinal))
                    {
                        user.UserName = seed.UserName;
                        user.NormalizedUserName = userManager.NormalizeName(seed.UserName);
                        changed = true;
                    }

                    if (!string.Equals(user.TwitchUserId, seed.TwitchUserId, StringComparison.Ordinal))
                    {
                        user.TwitchUserId = seed.TwitchUserId;
                        changed = true;
                    }

                    if (!string.Equals(user.TwitchLogin, seed.TwitchLogin, StringComparison.Ordinal))
                    {
                        user.TwitchLogin = seed.TwitchLogin;
                        changed = true;
                    }

                    if (changed)
                    {
                        await EnsureSuccess(await userManager.UpdateAsync(user), $"Не удалось обновить тестового пользователя '{seed.UserName}'");
                    }
                }

                if (!string.IsNullOrWhiteSpace(seed.Password))
                {
                    await SyncPasswordAsync(userManager, user, seed.Password);
                }

                IReadOnlyCollection<string> roles = await SyncRolesAsync(userManager, roleManager, user, seed.Roles);
                return new SeededAuthUser(user.Id, user.UserName!, seed.Password, user.TwitchUserId, user.TwitchLogin, roles);
            });

        private static async Task SyncPasswordAsync(
            UserManager<UserEntity> userManager,
            UserEntity user,
            string password)
        {
            if (await userManager.HasPasswordAsync(user))
            {
                if (await userManager.CheckPasswordAsync(user, password))
                {
                    return;
                }

                string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await EnsureSuccess(
                    await userManager.ResetPasswordAsync(user, resetToken, password),
                    $"Не удалось сбросить пароль тестового пользователя '{user.UserName}'");

                return;
            }

            await EnsureSuccess(
                await userManager.AddPasswordAsync(user, password),
                $"Не удалось установить пароль тестового пользователя '{user.UserName}'");
        }

        private static async Task<IReadOnlyCollection<string>> SyncRolesAsync(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            UserEntity user,
            IReadOnlyCollection<string> requestedRoles)
        {
            string[] desiredRoles = requestedRoles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (string role in desiredRoles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                IdentityRole<Guid> identityRole = new()
                {
                    Name = role,
                    NormalizedName = roleManager.NormalizeKey(role),
                };

                await EnsureSuccess(
                    await roleManager.CreateAsync(identityRole),
                    $"Не удалось создать роль '{role}' для тестового пользователя '{user.UserName}'");
            }

            IList<string> currentRoles = await userManager.GetRolesAsync(user);
            string[] rolesToRemove = currentRoles.Except(desiredRoles, StringComparer.Ordinal).ToArray();
            if (rolesToRemove.Length > 0)
            {
                await EnsureSuccess(
                    await userManager.RemoveFromRolesAsync(user, rolesToRemove),
                    $"Не удалось удалить роли у тестового пользователя '{user.UserName}'");
            }

            string[] rolesToAdd = desiredRoles.Except(currentRoles, StringComparer.Ordinal).ToArray();
            if (rolesToAdd.Length > 0)
            {
                await EnsureSuccess(
                    await userManager.AddToRolesAsync(user, rolesToAdd),
                    $"Не удалось назначить роли тестовому пользователю '{user.UserName}'");
            }

            return desiredRoles;
        }

        private static Task EnsureSuccess(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return Task.CompletedTask;
            }

            string details = string.Join("; ", result.Errors.Select(error => error.Description));

            throw new InvalidOperationException($"{message} {details}");
        }
    }
}
