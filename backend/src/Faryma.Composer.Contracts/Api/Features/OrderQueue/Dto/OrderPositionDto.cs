using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue.Dto
{
    /// <summary>
    /// Представляет позицию заказа в очереди, включая сам заказ и историю перемещений
    /// </summary>
    public sealed record OrderPositionDto
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Заказ был обновлен
        /// </summary>
        public required bool IsOrderUpdated { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }

        /// <summary>
        /// Позиция заказа в очереди была изменена
        /// </summary>
        public required bool IsPositionChanged { get; init; }

        public static OrderPositionDto Map(OrderPosition orderPosition)
        {
            return new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                IsOrderUpdated = orderPosition.IsOrderUpdated,
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
                IsPositionChanged = orderPosition.PositionHistory.IsPositionChanged,
            };
        }
    }
}