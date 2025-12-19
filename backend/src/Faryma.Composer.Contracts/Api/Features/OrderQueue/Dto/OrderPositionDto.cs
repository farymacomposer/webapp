using System.ComponentModel.DataAnnotations;
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
        [Required]
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        [Required]
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        [Required]
        public required OrderQueuePositionDto CurrentPosition { get; init; }

        public static OrderPositionDto Map(OrderPosition orderPosition)
        {
            return new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };
        }
    }
}