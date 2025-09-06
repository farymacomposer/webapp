using System.Text.Encodings.Web;
using System.Text.Json;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Faryma.Composer.Api
{
    public sealed class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is AppException appException)
            {
                ResultObject resultObject = appException.GetResultObject();
                context.Result = new JsonResult(resultObject, _jsonOptions)
                {
                    StatusCode = 600
                };
            }
        }
    }
}