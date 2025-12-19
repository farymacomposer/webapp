using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.UserNickname
{
    public sealed class UserNicknameService(UnitOfWork uow)
    {
        public async Task<UserNicknameEntity> GetOrCreate(string nickname)
        {
            UserNicknameEntity? result = await uow.UserNicknameWrite.Find(nickname);

            if (result is null)
            {
                result = uow.UserNicknameWrite.Create(nickname);
                uow.UserAccountWrite.Create(result);
                await uow.SaveChangesAsync();
            }

            return result;
        }
    }
}