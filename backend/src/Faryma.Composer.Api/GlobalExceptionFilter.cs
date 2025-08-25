using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Faryma.Composer.Api
{
    public sealed class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is AppException appException)
            {
                ResultObject resultObject = appException.GetResultObject();
                context.Result = new JsonResult(resultObject)
                {
                    StatusCode = 600
                };
            }
        }
    }
}