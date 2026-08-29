using System.Security.Authentication;

namespace Faryma.Composer.Infrastructure
{
    public sealed class CurrentUserContext
    {
        public Guid? UserId { get; private set; }

        public void SetUserId(Guid userId) => UserId = userId;

        public Guid GetRequiredUserId()
        {
            if (UserId is not Guid userId)
            {
                throw new AuthenticationException("Не удалось определить пользователя");
            }

            return userId;
        }
    }
}
