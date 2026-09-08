using System.Security.Authentication;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Common.Exceptions
{
    public sealed class ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            switch (exception)
            {
                case AuthenticationException:
                {
                    await WriteJson(httpContext, StatusCodes.Status401Unauthorized, new { Message = "Ошибка аутентификации" }, cancellationToken);

                    return true;
                }
                case AppException appException:
                {
                    logger.LogInformation("Business error {Code}: {Message} {@Context}", appException.Code, appException.Message, appException.Context);

                    ProblemDetails problemDetails = new()
                    {
                        Type = $"urn:composer:error:{appException.Code}",
                        Title = "Ошибка бизнес-логики",
                        Status = appException.StatusCode,
                        Detail = appException.Message,
                    };

                    httpContext.Response.StatusCode = appException.StatusCode;

                    return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        Exception = exception,
                        ProblemDetails = problemDetails
                    });
                }
                default:
                {
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
        }

        private static async Task WriteJson(HttpContext httpContext, int statusCode, object payload, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, payload, _jsonOptions, cancellationToken);
        }
    }
}
