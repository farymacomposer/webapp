using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Api.Common.Middleware
{
    public sealed class RequestContextMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            _ = context.RequestServices.GetRequiredService<DateTimeContext>();

            if (context.User.Identity?.IsAuthenticated == true)
            {
                CurrentUserContext currentUserContext = context.RequestServices.GetRequiredService<CurrentUserContext>();
                Guid userId = context.User.GetUserId();
                currentUserContext.SetUserId(userId);
            }

            await next(context);
        }
    }
}
