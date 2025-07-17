using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.UserNicknameFeature
{
    public sealed class UserNicknameService(UnitOfWork ofw)
    {
        public async Task<UserNickname> GetOrCreate(string nickname)
        {
            UserNickname? result = await ofw.UserNicknameRepository.Find(nickname);

            if (result is null)
            {
                result = ofw.UserNicknameRepository.Create(nickname);
                ofw.UserAccountRepository.Create(result);
                await ofw.SaveChangesAsync();
            }

            return result;
        }
    }
}