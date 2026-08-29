using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.UserNickname;

namespace Faryma.Composer.Application.Features.UserNickname
{
    public sealed class UserNicknameService(
        AppDbContext appDbContext,
        UserNicknameStore userNicknameStore)
    {
        public async Task<UserNicknameEntity> GetOrCreate(string nickname, CancellationToken ct)
        {
            UserNicknameEntity? result = await userNicknameStore.FindByNickname(nickname, ct);

            if (result is null)
            {
                result = userNicknameStore.Create(nickname);
                await appDbContext.SaveChangesAsync(ct);
            }

            return result;
        }
    }
}
