using Faryma.Composer.Contracts.Api.Features.Auth.Options;
using Faryma.Composer.Contracts.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Features.Auth.Services
{
    public sealed class AdminBootstrapService(
        UserManager<UserEntity> userManager,
        AuthTokenService authTokenService,
        IOptions<AdminBootstrapOptions> options)
    {
        public async Task Initialize()
        {
            await SyncAdminAccount(options.Value.Composer, AppRoles.Composer);
            await SyncAdminAccount(options.Value.Moderator, AppRoles.Moderator);
        }

        private static Task EnsureSuccess(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return Task.CompletedTask;
            }

            string details = string.Join("; ", result.Errors.Select(x => x.Description));

            throw new InvalidOperationException($"{message} {details}");
        }

        private async Task SyncAdminAccount(AdminBootstrapAccountOptions account, string targetRole)
        {
            string targetName = account.UserName;
            UserEntity? userByRole = await GetSingleUserInRole(targetRole);

            UserEntity user;
            if (userByRole is not null)
            {
                if (!string.Equals(userByRole.UserName, targetName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Невозможно запустить bootstrap администратора для роли '{targetRole}', так как в базе уже существует учетная запись '{userByRole.UserName}', а в env указан логин '{targetName}'");
                }

                user = userByRole;
            }
            else if (await userManager.FindByNameAsync(targetName) is not null)
            {
                throw new InvalidOperationException($"Невозможно запустить bootstrap администратора для роли '{targetRole}', так как логин '{targetName}' уже существует в базе, но не назначен на эту роль");
            }
            else
            {
                user = new UserEntity
                {
                    UserName = targetName,
                    NormalizedUserName = userManager.NormalizeName(targetName),
                    CreatedAt = DateTime.UtcNow
                };

                await EnsureSuccess(await userManager.CreateAsync(user), $"Не удалось создать аккаунт администратора для роли '{targetRole}'");
            }

            bool passwordChanged = await SyncPassword(user, account.Password);
            await EnsureRole(user, targetRole);

            if (passwordChanged)
            {
                await authTokenService.RevokeAll(user.Id);
            }
        }

        private async Task<UserEntity?> GetSingleUserInRole(string role)
        {
            IList<UserEntity> users = await userManager.GetUsersInRoleAsync(role);

            return users.Count switch
            {
                0 => null,
                1 => users[0],
                _ => throw new InvalidOperationException($"Невозможно запустить bootstrap администраторов, так как роль '{role}' назначена нескольким пользователям")
            };
        }

        private async Task<bool> SyncPassword(UserEntity user, string password)
        {
            IdentityResult result;

            if (await userManager.HasPasswordAsync(user))
            {
                if (await userManager.CheckPasswordAsync(user, password))
                {
                    return false;
                }

                result = await ResetPassword(user, password);
            }
            else
            {
                result = await userManager.AddPasswordAsync(user, password);
            }

            await EnsureSuccess(result, $"Не удалось синхронизировать пароль для аккаунта администратора '{user.UserName}'");

            return true;
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

            await EnsureSuccess(await userManager.AddToRoleAsync(user, role), $"Не удалось назначить роль '{role}' для аккаунта администратора '{user.UserName}'");
        }
    }
}
