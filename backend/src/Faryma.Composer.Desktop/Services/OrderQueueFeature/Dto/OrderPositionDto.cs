using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto
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
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }
    }
}