using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class UserAccountRepository(AppDbContext context)
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