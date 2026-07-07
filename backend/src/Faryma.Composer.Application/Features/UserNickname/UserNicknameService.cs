using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.UserNickname
{
    public sealed class UserNicknameService(UnitOfWork uow)
    {
        public async Task<UserNicknameEntity> GetOrCreate(string nickname, CancellationToken ct)
        {
            UserNicknameEntity? result = await uow.UserNicknameStore.FindByNickname(nickname, ct);

            if (result is null)
            {
                result = uow.UserNicknameStore.Create(nickname);
                await uow.SaveChanges(ct);
            }

            return result;
        }
    }
}
