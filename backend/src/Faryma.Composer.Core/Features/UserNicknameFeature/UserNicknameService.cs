using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.UserNicknameFeature
{
    public sealed class UserNicknameService(UnitOfWork uow)
    {
        public async Task<UserNickname> GetOrCreate(string nickname)
        {
            UserNickname? result = await uow.UserNicknameRepository.Find(nickname);

            if (result is null)
            {
                result = uow.UserNicknameRepository.Create(nickname);
                uow.UserAccountRepository.Create(result);
                await uow.SaveChangesAsync();
            }

            return result;
        }
    }
}