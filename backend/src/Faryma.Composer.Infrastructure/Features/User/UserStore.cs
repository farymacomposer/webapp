using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.User
{
    public sealed class UserStore(UserManager<UserEntity> userManager)
    {
        public async Task<UserEntity> GetUser(Guid userId, CancellationToken ct)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new NotFoundException($"Пользователь с id: {userId} не найден");
        }
    }
}
