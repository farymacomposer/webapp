using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class UserNicknameStore(AppDbContext context, ILookupNormalizer normalizer)
    {
        public UserNicknameEntity Create(string nickname)
        {
            UserNicknameEntity result = new()
            {
                Nickname = nickname,
                NormalizedNickname = normalizer.NormalizeName(nickname),
            };

            context.Add(result);
            context.Add(new UserAccountEntity
            {
                UserNickname = result
            });

            return result;
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