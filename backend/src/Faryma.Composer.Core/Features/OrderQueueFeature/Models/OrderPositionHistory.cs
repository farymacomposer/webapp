namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Представляет историю изменений позиции заказа в очереди, включая предыдущее и текущее состояние
    /// </summary>
    public sealed class OrderPositionHistory
    {
        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public OrderQueuePosition Previous { get; init; } = new();

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public OrderQueuePosition Current { get; init; } = new();

        /// <summary>
        /// Позиция заказа в очереди была изменена более чем на один шаг
        /// </summary>
        public bool IsPositionJumped => Math.Abs(Previous.QueueIndex - Current.QueueIndex) > 1;

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