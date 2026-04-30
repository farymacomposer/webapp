using Faryma.Composer.Api.Common.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IdempotentAttribute : TypeFilterAttribute
    {
        public IdempotentAttribute() : base(typeof(IdempotentFilter))
        {
        }
    }
}
