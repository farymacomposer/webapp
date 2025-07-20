namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public enum OrderActivityStatus
    {
        Initial = 0,
        Future = 1,
        Active = 2,
        Inactive = 3
    }

    /// <summary>
    /// Позиция заказа в очереди на разбор
    /// </summary>
    public sealed class OrderPosition
    {
        /// <summary>
        /// Предыдущая позиция в очереди
        /// </summary>
        public int PrevIndex { get; set; }

        /// <summary>
        /// Текущая позиция в очереди
        /// </summary>
        public int CurrentIndex { get; set; }

        public OrderActivityStatus PrevActivityStatus { get; set; }

        public OrderActivityStatus CurrentActivityStatus { get; set; }
    }
}