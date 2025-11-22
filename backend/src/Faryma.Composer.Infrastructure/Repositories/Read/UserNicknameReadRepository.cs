using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Read
{
    public sealed class UserNicknameReadRepository(AppDbContext context)
    {
        public Task<bool> HasOrders(UserNicknameEntity userNickname) => context.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0);
    }
}