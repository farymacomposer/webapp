using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;

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
    }
}