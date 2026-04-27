using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Contracts.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Faryma.Composer.Api.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IdempotentAttribute : Attribute, IAsyncActionFilter
    {
        private static readonly TimeSpan _expiration = TimeSpan.FromMinutes(10);

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(Globals.IdempotencyKey, out StringValues raw))
            {
                context.Result = new BadRequestObjectResult($"Требуется заголовок {Globals.IdempotencyKey}");

                return;
            }

            if (!Guid.TryParse(raw, out Guid idempotencyKey))
            {
                context.Result = new BadRequestObjectResult($"Некорректный заголовок {Globals.IdempotencyKey}");

                return;
            }

            if (idempotencyKey == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult($"Пустой заголовок {Globals.IdempotencyKey}");

                return;
            }

            Endpoint? endpoint = context.HttpContext.GetEndpoint();
            string routePattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText
                ?? context.HttpContext.Request.Path.Value
                    ?? "unknown";

            string userScope = (context.HttpContext.User.Identity?.IsAuthenticated == true)
                ? context.HttpContext.User.GetUserId().ToString("D")
                : "anonymous";

            IMemoryCache cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            string cacheKey = $"Idempotent:{routePattern}:{userScope}:{idempotencyKey}";

            Task<object?> task = cache.GetOrCreate(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _expiration;

                ActionExecutedContext executed = await next();

                if (executed.Result is OkObjectResult objectResult)
                {
                    return objectResult.Value;
                }

                return null;
            })!;

            object? resultValue = await task;

            if (resultValue is not null)
            {
                context.Result = new OkObjectResult(resultValue);
            }
            else
            {
                cache.Remove(cacheKey);
            }
        }
    }
}
