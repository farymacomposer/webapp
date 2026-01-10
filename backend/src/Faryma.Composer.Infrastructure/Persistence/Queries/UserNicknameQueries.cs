using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Queries
{
    public sealed class UserNicknameQueries(AppDbContext context)
    {
        public Task<bool> HasOrders(UserNicknameEntity userNickname) =>
            context.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0);
    }
}