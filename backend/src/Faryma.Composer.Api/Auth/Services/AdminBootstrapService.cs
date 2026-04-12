using Faryma.Composer.Contracts.Api.Auth.Options;
using Faryma.Composer.Contracts.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class AdminBootstrapService(
        UserManager<UserEntity> userManager,
        IOptions<AdminBootstrapOptions> options,
        ILogger<AdminBootstrapService> logger)
    {
        public async Task Initialize(CancellationToken ct = default)
        {
            await SyncAdminAccount(options.Value.Composer, AppRoles.Composer, AppRoles.Moderator, ct);
            await SyncAdminAccount(options.Value.Moderator, AppRoles.Moderator, AppRoles.Composer, ct);
        }

        private static Task EnsureSuccess(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return Task.CompletedTask;
            }

            string details = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"{message} {details}".Trim());
        }

        private async Task SyncAdminAccount(
                    AdminBootstrapAccountOptions account,
            string targetRole,
            string otherAdminRole,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            string targetUserName = account.UserName.Trim();
            UserEntity? roleUser = await GetSingleUserInRole(targetRole);
            UserEntity? userByName = await userManager.FindByNameAsync(targetUserName);

            UserEntity user;
            if (roleUser is not null)
            {
                if (userByName is not null && userByName.Id != roleUser.Id)
                {
                    throw new InvalidOperationException(
                        $"Cannot sync '{targetRole}' account because username '{targetUserName}' is already used by another user.");
                }

                user = roleUser;
                await SyncUserName(user, targetUserName);
            }
            else if (userByName is not null)
            {
                if (await userManager.IsInRoleAsync(userByName, otherAdminRole))
                {
                    throw new InvalidOperationException(
                        $"Cannot sync '{targetRole}' account because username '{targetUserName}' is already assigned to '{otherAdminRole}'.");
                }

                user = userByName;
            }
            else
            {
                user = new UserEntity
                {
                    UserName = targetUserName,
                    NormalizedUserName = userManager.NormalizeName(targetUserName),
                    CreatedAt = DateTime.UtcNow
                };

                await EnsureSuccess(
                    await userManager.CreateAsync(user),
                    $"Failed to create '{targetRole}' account.");
            }

            await SyncPassword(user, account.Password);
            await EnsureRole(user, AppRoles.User);
            await EnsureRole(user, targetRole);
            await RemoveRoleIfAssigned(user, otherAdminRole);

            logger.LogInformation("Administrative account synchronized for role {Role}.", targetRole);
        }

        private async Task<UserEntity?> GetSingleUserInRole(string role)
        {
            IList<UserEntity> users = await userManager.GetUsersInRoleAsync(role);
            return users.Count switch
            {
                0 => null,
                1 => users[0],
                _ => throw new InvalidOperationException(
                    $"Cannot bootstrap administrators because role '{role}' is assigned to multiple users.")
            };
        }

        private async Task SyncUserName(UserEntity user, string targetUserName)
        {
            if (string.Equals(user.UserName, targetUserName, StringComparison.Ordinal))
            {
                return;
            }

            user.UserName = targetUserName;
            user.NormalizedUserName = userManager.NormalizeName(targetUserName);

            await EnsureSuccess(
                await userManager.UpdateAsync(user),
                $"Failed to update username for administrative account '{targetUserName}'.");
        }

        private async Task SyncPassword(UserEntity user, string password)
        {
            IdentityResult result = await userManager.HasPasswordAsync(user)
                ? await ResetPassword(user, password)
                : await userManager.AddPasswordAsync(user, password);

            await EnsureSuccess(result, $"Failed to synchronize password for administrative account '{user.UserName}'.");
        }

        private async Task<IdentityResult> ResetPassword(UserEntity user, string password)
        {
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            return await userManager.ResetPasswordAsync(user, resetToken, password);
        }

        private async Task EnsureRole(UserEntity user, string role)
        {
            if (await userManager.IsInRoleAsync(user, role))
            {
                return;
            }

            await EnsureSuccess(
                await userManager.AddToRoleAsync(user, role),
                $"Failed to assign role '{role}' to administrative account '{user.UserName}'.");
        }

        private async Task RemoveRoleIfAssigned(UserEntity user, string role)
        {
            if (!await userManager.IsInRoleAsync(user, role))
            {
                return;
            }

            await EnsureSuccess(
                await userManager.RemoveFromRoleAsync(user, role),
                $"Failed to remove role '{role}' from administrative account '{user.UserName}'.");
        }
    }
}