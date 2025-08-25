using Faryma.Composer.Core.Features.ReviewOrderFeature;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Faryma.Composer.Api
{
    public sealed class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            object? resultObject = context.Exception switch
            {
                ReviewOrderException ex => ex.GetResultObject(),
                _ => null
            };

            if (resultObject is not null)
            {
                context.Result = new JsonResult(resultObject)
                {
                    StatusCode = 600
                };
            }
        }
    }
}