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
        public required TimeSpan PositionsHashCode { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }

        public static OrderPositionsChangedEvent Map(TimeSpan positionsHashCode, IEnumerable<OrderPosition> positions)
        {
            return new()
            {
                PositionsHashCode = positionsHashCode,
                OrderPositions = positions.Select(OrderPositionDto.Map),
            };
        }
    }
}