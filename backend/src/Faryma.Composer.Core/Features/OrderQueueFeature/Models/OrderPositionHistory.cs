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
        public required OrderQueuePosition Previous { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePosition Current { get; init; }

        /// <summary>
        /// Позиция заказа в очереди была изменена более чем на один шаг
        /// </summary>
        public bool IsPositionJumped => Math.Abs(Previous.QueueIndex - Current.QueueIndex) > 1;

        public static OrderPositionHistory Create()
        {
            return new()
            {
                Current = new OrderQueuePosition
                {
                    Category = new OrderCategoryInfo()
                },
                Previous = new OrderQueuePosition
                {
                    Category = new OrderCategoryInfo()
                }
            };
        }

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