namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    /// <summary>
    /// Позиция заказа в очереди на разбор
    /// </summary>
    public sealed class OrderPosition
    {
        /// <summary>
        /// Предыдущая позиция в очереди
        /// </summary>
        public int Prev { get; set; }

        /// <summary>
        /// Текущая позиция в очереди
        /// </summary>
        public int Current { get; set; }
    }
}