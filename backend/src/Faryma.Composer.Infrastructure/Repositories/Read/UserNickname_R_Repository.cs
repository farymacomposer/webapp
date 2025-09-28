using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Read
{
    public sealed class UserNickname_R_Repository(AppDbContext context)
    {
        public Task<bool> HasOrders(UserNickname userNickname) => context.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0);
    }
}