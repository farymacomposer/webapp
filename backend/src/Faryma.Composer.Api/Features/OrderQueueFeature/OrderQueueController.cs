using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Api.Features.OrderQueueFeature.GetOrderQueue;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    /// <summary>
    /// Работа с очередью заказов
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class OrderQueueController(OrderQueueService orderQueueService) : ControllerBase
    {
        /// <summary>
        /// Получает текущее состояние очереди заказов
        /// </summary>
        [HttpGet(nameof(GetOrderQueue))]
        public async Task<ActionResult<GetOrderQueueResponse>> GetOrderQueue()
        {
            Dictionary<long, OrderPosition> orders = await orderQueueService.GetOrderQueue();

            return Ok(new GetOrderQueueResponse
            {
                Items = orders.Select(x => OrderPositionDto.Map(x.Value))
            });
        }
    }
}