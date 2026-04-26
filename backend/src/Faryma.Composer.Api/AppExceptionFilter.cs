using System.Security.Authentication;
using System.Text.Encodings.Web;
using System.Text.Json;
using Faryma.Composer.Contracts.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Faryma.Composer.Api
{
    public sealed class AppExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is AuthenticationException)
            {
                context.ExceptionHandled = true;
                context.Result = new JsonResult(new { Message = "Ошибка аутентификации" }, _jsonOptions)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };

                return;
            }

            if (context.Exception is AppException appException)
            {
                ResultObject resultObject = appException.GetResultObject();
                context.ExceptionHandled = true;
                context.Result = new JsonResult(resultObject, _jsonOptions)
                {
                    StatusCode = AppException.StatusCode
                };
            }
        }
    }
}
