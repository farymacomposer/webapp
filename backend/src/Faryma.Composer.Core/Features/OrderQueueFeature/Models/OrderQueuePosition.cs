using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Позиция заказа в очереди на разбор
    /// </summary>
    public sealed class OrderQueuePosition
    {
        /// <summary>
        /// Предыдущая позиция в очереди
        /// </summary>
        public int PrevIndex { get; set; }

        /// <summary>
        /// Текущая позиция в очереди
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// Предыдущий статус активности заказа
        /// </summary>
        public OrderActivityStatus PrevActivityStatus { get; set; }

        /// <summary>
        /// Текущий статус активности заказа
        /// </summary>
        public OrderActivityStatus CurrentActivityStatus { get; set; }
    }
}