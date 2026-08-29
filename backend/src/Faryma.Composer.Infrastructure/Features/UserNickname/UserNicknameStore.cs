using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.UserNickname
{
    public sealed class UserNicknameStore(AppDbContext appDbContext, ILookupNormalizer normalizer)
    {
        public UserNicknameEntity Create(string nickname)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nickname);

            UserNicknameEntity result = new()
            {
                Nickname = nickname,
                NormalizedNickname = normalizer.NormalizeName(nickname),
            };

            appDbContext.Add(result);
            appDbContext.Add(new UserNicknameAccountEntity
            {
                UserNickname = result
            });

            return result;
        }

        public Task<UserNicknameEntity?> FindByNickname(string nickname, CancellationToken ct)
        {
            string normalized = normalizer.NormalizeName(nickname);

            return appDbContext.UserNicknames
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.NormalizedNickname == normalized, ct);
        }
    }
}
