using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Добавлен новый заказ
    /// </summary>
    public sealed record NewOrderAddedEvent
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }
    }
}