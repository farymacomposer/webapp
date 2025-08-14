namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Очередь заказов
    /// </summary>
    public sealed record OrderQueue
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPosition> Positions { get; init; }
    }
}