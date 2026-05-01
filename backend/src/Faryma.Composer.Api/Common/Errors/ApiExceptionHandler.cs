using System.Security.Authentication;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Faryma.Composer.Contracts.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Common.Errors
{
    public sealed class ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            switch (exception)
            {
                case AuthenticationException:
                    await WriteJson(httpContext, StatusCodes.Status401Unauthorized, new { Message = "Ошибка аутентификации" }, cancellationToken);

                    return true;

                case AppException appException:
                    await WriteJson(httpContext, AppException.StatusCode, appException.GetResultObject(), cancellationToken);

                    return true;

                default:
                    logger.LogCritical(exception, "Необработанное исключение API");

                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        Exception = exception,
                        ProblemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status500InternalServerError,
                            Title = "Произошла непредвиденная ошибка"
                        }
                    });
            }
        }

        private static async Task WriteJson(HttpContext httpContext, int statusCode, object payload, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, payload, _jsonOptions, cancellationToken);
        }
    }
}
