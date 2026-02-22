using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class UserStore(AppDbContext context)
    {
        public Task<UserEntity?> FindById(Guid id, CancellationToken ct) =>
            context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}