using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Элемент в очереди заказов, содержит информацию о заказе и его позиции в очереди
    /// </summary>
    public sealed record OrderQueueItem
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrder Order { get; init; }

        /// <summary>
        /// Позиция заказа в очереди, включая его индекс, статус активности и категорию
        /// </summary>
        public required OrderQueuePosition Position { get; init; }
    }
}