using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.UserNickname
{
    public sealed class UserNicknameService(UnitOfWork uow)
    {
        public async Task<UserNicknameEntity> GetOrCreate(string nickname)
        {
            UserNicknameEntity? result = await uow.UserNicknameStore.Find(nickname);

            if (result is null)
            {
                result = uow.UserNicknameStore.Create(nickname);
                await uow.SaveChanges();
            }

            return result;
        }
    }
}