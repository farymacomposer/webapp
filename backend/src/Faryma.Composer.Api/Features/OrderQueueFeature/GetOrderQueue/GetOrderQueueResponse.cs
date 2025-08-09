using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.GetOrderQueue
{
    /// <summary>
    /// Ответ на запрос получения очереди заказов
    /// </summary>
    public sealed record GetOrderQueueResponse
    {
        /// <summary>
        /// Коллекция позиций заказов в очереди
        /// </summary>
        public required IEnumerable<OrderPositionDto> Items { get; set; }
    }
}