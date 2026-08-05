using System.Runtime.CompilerServices;
using System.Security.Authentication;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    [ApiController]
    [Route("api/_test/exceptions")]
    public sealed class TestExceptionProbeController : ControllerBase
    {
        [HttpGet("app")]
        public IActionResult ThrowAppException() => throw new TestApiException();

        [HttpGet("authentication")]
        public IActionResult ThrowAuthenticationException() => throw new AuthenticationException("Тестовая ошибка аутентификации");

        [HttpGet("unhandled")]
        public IActionResult ThrowUnhandledException() => throw new InvalidOperationException("Тестовая непредвиденная ошибка");
    }

    internal sealed class TestApiException([CallerMemberName] string callerMemberName = "")
        : AppException("Тестовая ошибка API", callerMemberName);
}
