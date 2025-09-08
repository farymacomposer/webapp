using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.UserNicknameFeature
{
    public sealed class UserNicknameService(UnitOfWork uow)
    {
        public async Task<UserNickname> GetOrCreate(string nickname)
        {
            UserNickname? result = await uow.UserNickname_RW.Find(nickname);

            if (result is null)
            {
                result = uow.UserNickname_RW.Create(nickname);
                uow.UserAccount_RW.Create(result);
                await uow.SaveChangesAsync();
            }

            return result;
        }
    }
}