namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Представляет историю изменений позиции заказа в очереди, включая предыдущее и текущее состояние
    /// </summary>
    public sealed record OrderPositionHistory
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