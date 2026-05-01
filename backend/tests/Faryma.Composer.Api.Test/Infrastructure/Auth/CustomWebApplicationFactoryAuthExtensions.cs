using System.Net.Http.Headers;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

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

            SeededAuthUser admin = await app.EnsureUserAsync(seed, ct);
            if (string.IsNullOrWhiteSpace(admin.Password))
            {
                throw new InvalidOperationException("Admin bearer helper requires a seeded password.");
            }

            await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
            AuthTokenService authTokenService = scope.ServiceProvider.GetRequiredService<AuthTokenService>();
            UserManager<UserEntity> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
            UserEntity user = await userManager.FindByIdAsync(admin.UserId.ToString())
                ?? throw new InvalidOperationException($"Failed to load seeded admin user '{admin.UserName}'.");
            (string accessToken, _) = await authTokenService.IssueForUser(user, ct);

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
                        .AddScheme<AuthenticationSchemeOptions, BrowserUserTestAuthHandler>(
                            BrowserUserTestAuthHandler.SchemeName,
                            _ => { });
                });
            });

            HttpClient client = browserApp.CreateAnonymousClient();
            SeededAuthUser seededUser = await browserApp.EnsureUserAsync(seed, ct);
            stateHolder.State = new BrowserUserAuthenticationState(
                seededUser.UserId,
                seededUser.UserName,
                seededUser.TwitchUserId,
                seededUser.TwitchLogin,
                seededUser.Roles);

            return client;
        }

        private static async Task<SeededAuthUser> EnsureUserAsync(
            this CustomWebApplicationFactory app,
            TestAuthUserSeed seed,
            CancellationToken ct)
        {
            await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
            UserManager<UserEntity> userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
            RoleManager<IdentityRole<Guid>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

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

                await EnsureSuccess(await userManager.CreateAsync(user), $"Failed to create test user '{seed.UserName}'.");
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
                    await EnsureSuccess(await userManager.UpdateAsync(user), $"Failed to update test user '{seed.UserName}'.");
                }
            }

            if (!string.IsNullOrWhiteSpace(seed.Password))
            {
                await SyncPasswordAsync(userManager, user, seed.Password);
            }

            IReadOnlyCollection<string> roles = await SyncRolesAsync(userManager, roleManager, user, seed.Roles);
            return new SeededAuthUser(user.Id, user.UserName!, seed.Password, user.TwitchUserId, user.TwitchLogin, roles);
        }

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
                    $"Failed to reset password for test user '{user.UserName}'.");

                return;
            }

            await EnsureSuccess(
                await userManager.AddPasswordAsync(user, password),
                $"Failed to set password for test user '{user.UserName}'.");
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
                    $"Failed to create role '{role}' for test user '{user.UserName}'.");
            }

            IList<string> currentRoles = await userManager.GetRolesAsync(user);
            string[] rolesToRemove = currentRoles.Except(desiredRoles, StringComparer.Ordinal).ToArray();
            if (rolesToRemove.Length > 0)
            {
                await EnsureSuccess(
                    await userManager.RemoveFromRolesAsync(user, rolesToRemove),
                    $"Failed to remove roles from test user '{user.UserName}'.");
            }

            string[] rolesToAdd = desiredRoles.Except(currentRoles, StringComparer.Ordinal).ToArray();
            if (rolesToAdd.Length > 0)
            {
                await EnsureSuccess(
                    await userManager.AddToRolesAsync(user, rolesToAdd),
                    $"Failed to assign roles to test user '{user.UserName}'.");
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
