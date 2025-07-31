using Faryma.Composer.Api.Auth;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    /// <summary>
    ///
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class OrderQueueController(OrderQueueService orderQueueService) : ControllerBase
    {
        /// <summary>
        ///
        /// </summary>
        [HttpPost(nameof(GetOrderQueue))]
        [AuthorizeComposer]
        public async Task<ActionResult<GetOrderQueueResponse>> GetOrderQueue()
        {
            IReadOnlyDictionary<long, OrderPosition> orders = orderQueueService.GetOrderQueue();

            return Ok(GetOrderQueueResponse.Map(orders));
        }
    }
}