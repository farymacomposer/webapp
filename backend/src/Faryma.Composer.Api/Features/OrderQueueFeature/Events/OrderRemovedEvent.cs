using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Заказ удален
    /// </summary>
    public sealed record OrderRemovedEvent
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        public static OrderRemovedEvent Map(OrderPosition orderPosition)
        {
            return new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };
        }
    }
}