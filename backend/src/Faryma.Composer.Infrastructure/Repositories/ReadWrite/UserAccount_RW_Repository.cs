using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class UserAccount_RW_Repository(AppDbContext context)
    {
        public UserAccount Create(UserNickname userNickname)
        {
            return context.Add(new UserAccount
            {
                UserNickname = userNickname
            }).Entity;
        }
    }
}