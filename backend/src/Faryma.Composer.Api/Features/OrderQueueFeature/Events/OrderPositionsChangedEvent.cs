using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Изменены позиции заказов
    /// </summary>
    public sealed record OrderPositionsChangedEvent
    {
        /// <summary>
        /// Хэш-код позиций заказов
        /// </summary>
        public required int PositionsHashCode { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }

        public static OrderPositionsChangedEvent Map(OrderQueue orderQueue)
        {
            return new()
            {
                PositionsHashCode = orderQueue.PositionsHashCode,
                OrderPositions = orderQueue.Positions.Select(OrderPositionDto.Map),
            };
        }
    }
}