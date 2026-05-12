using Faryma.Composer.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    [ApiController]
    [Route("api/_test/auth")]
    public sealed class TestAuthProbeController : ControllerBase
    {
        [HttpGet("authenticated")]
        [Authorize]
        public ActionResult<object> Authenticated()
        {
            return Ok(new
            {
                User.Identity?.Name,
            });
        }

        [HttpGet("admin")]
        [AuthorizeAdmins]
        public ActionResult<object> Admin()
        {
            return Ok(new
            {
                User.Identity?.Name,
            });
        }

        [HttpPost("rate-limited-login")]
        [EnableRateLimiting("auth-login")]
        public IActionResult RateLimitedLogin()
        {
            return Unauthorized();
        }
    }
}
