using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class UserAccount_RW_Repository(AppDbContext context)
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