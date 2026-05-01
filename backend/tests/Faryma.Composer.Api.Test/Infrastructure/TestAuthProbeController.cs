using Faryma.Composer.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
