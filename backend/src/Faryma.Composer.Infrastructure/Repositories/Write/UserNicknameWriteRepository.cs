using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class UserNicknameWriteRepository(AppDbContext context, ILookupNormalizer normalizer)
    {
        public UserNicknameEntity Create(string nickname)
        {
            return context.Add(new UserNicknameEntity
            {
                Nickname = nickname,
                NormalizedNickname = normalizer.NormalizeName(nickname),
            }).Entity;
        }

        public Task<UserNicknameEntity?> Find(string nickname)
        {
            string normalized = normalizer.NormalizeName(nickname);

            return context.UserNicknames
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.NormalizedNickname == normalized);
        }
    }
}