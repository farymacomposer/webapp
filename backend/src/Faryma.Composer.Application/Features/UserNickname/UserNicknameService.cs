using Faryma.Composer.Contracts;
using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Application.Features.UserNicknameFeature
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