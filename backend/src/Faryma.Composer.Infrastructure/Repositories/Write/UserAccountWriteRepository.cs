using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class UserAccountWriteRepository(AppDbContext context)
    {
        public UserAccountEntity Create(UserNicknameEntity userNickname)
        {
            return context.Add(new UserAccountEntity
            {
                UserNickname = userNickname
            }).Entity;
        }
    }
}