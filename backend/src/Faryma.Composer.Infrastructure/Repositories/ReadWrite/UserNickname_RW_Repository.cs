using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class UserNickname_RW_Repository(AppDbContext context, ILookupNormalizer normalizer)
    {
        public UserNickname Create(string nickname)
        {
            return context.Add(new UserNickname
            {
                Nickname = nickname,
                NormalizedNickname = normalizer.NormalizeName(nickname),
            }).Entity;
        }

        public Task<UserNickname?> Find(string nickname)
        {
            string normalized = normalizer.NormalizeName(nickname);

            return context.UserNicknames
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.NormalizedNickname == normalized);
        }
    }
}