using System.Security.Authentication;
using System.Security.Claims;

namespace Faryma.Composer.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            string? rawUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(rawUserId) || !Guid.TryParse(rawUserId, out Guid userId))
            {
                throw new AuthenticationException("Не удалось определить пользователя");
            }

            return userId;
        }
    }
}
