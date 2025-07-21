using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    /// <summary>
    /// Позиция заказа в очереди на разбор
    /// </summary>
    public sealed record OrderQueuePositionDto
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