using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/_test/request-context")]
    public sealed class TestRequestContextController(
        CurrentUserContext currentUserContext,
        DateTimeContext dateTimeContext) : ControllerBase
    {
        [HttpGet]
        public ActionResult<TestRequestContextResponse> Get()
        {
            Guid? claimsUserId = User.Identity?.IsAuthenticated == true
                ? User.GetUserId()
                : null;

            return Ok(new TestRequestContextResponse(currentUserContext.UserId, claimsUserId, dateTimeContext.Now));
        }
    }

    public sealed record TestRequestContextResponse(Guid? UserId, Guid? ClaimsUserId, DateTime Now);
}
