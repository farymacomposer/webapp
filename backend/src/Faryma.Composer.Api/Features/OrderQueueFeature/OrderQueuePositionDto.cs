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
        public required int PrevIndex { get; init; }

        /// <summary>
        /// Текущая позиция в очереди
        /// </summary>
        public required int CurrentIndex { get; init; }

        /// <summary>
        /// Предыдущий статус активности заказа
        /// </summary>
        public required OrderActivityStatus PrevActivityStatus { get; init; }

        /// <summary>
        /// Текущий статус активности заказа
        /// </summary>
        public required OrderActivityStatus CurrentActivityStatus { get; init; }
    }
}