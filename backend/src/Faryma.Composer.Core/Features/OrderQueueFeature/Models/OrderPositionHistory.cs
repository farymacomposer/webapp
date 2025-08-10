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

        public OrderPositionHistory Clone()
        {
            return new()
            {
                Previous = Previous.Clone(),
                Current = Current.Clone(),
            };
        }

        public override int GetHashCode() => HashCode.Combine(Previous, Current);
    }
}