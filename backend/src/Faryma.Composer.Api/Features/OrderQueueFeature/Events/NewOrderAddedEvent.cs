using Faryma.Composer.Api.Features.CommonDto;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Добавлен новый заказ
    /// </summary>
    public sealed record NewOrderAddedEvent
    {
        /// <summary>
        /// Хэш-код позиций заказов
        /// </summary>
        public required TimeSpan PositionsHashCode { get; init; }

        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }

        public static NewOrderAddedEvent Map(TimeSpan positionsHashCode, OrderPosition orderPosition)
        {
            return new()
            {
                PositionsHashCode = positionsHashCode,
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };
        }
    }
}