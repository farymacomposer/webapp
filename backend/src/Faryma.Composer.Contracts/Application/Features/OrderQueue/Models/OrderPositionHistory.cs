namespace Faryma.Composer.Contracts.Application.Features.OrderQueue.Models
{
    /// <summary>
    /// Представляет историю изменений позиции заказа в очереди, включая предыдущее и текущее состояние
    /// </summary>
    public sealed class OrderPositionHistory
    {
        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePosition Previous { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePosition Current { get; init; }

        /// <summary>
        /// Позиция заказа в очереди была изменена
        /// </summary>
        public bool IsPositionChanged => Previous.ActivityStatus != Current.ActivityStatus
            || Previous.QueueIndex != Current.QueueIndex
            || Previous.Category != Current.Category;

        public OrderPositionHistory Clone()
        {
            return new()
            {
                Previous = Previous.Clone(),
                Current = Current.Clone(),
            };
        }
    }
}
