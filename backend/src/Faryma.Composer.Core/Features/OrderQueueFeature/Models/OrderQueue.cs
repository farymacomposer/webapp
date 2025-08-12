namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Очередь заказов
    /// </summary>
    public sealed record OrderQueue
    {
        /// <summary>
        /// Хэш-код позиций заказов
        /// </summary>
        public required int PositionsHashCode { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPosition> Positions { get; init; }
    }
}