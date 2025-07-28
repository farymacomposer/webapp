namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Представляет изменение позиции заказа в очереди между двумя состояниями
    /// </summary>
    public sealed class OrderPositionTracker
    {
        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public OrderQueuePosition Previous { get; } = new();

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public OrderQueuePosition Current { get; } = new();
    }
}