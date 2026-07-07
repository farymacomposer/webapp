using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;

namespace Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models
{
    /// <summary>
    /// Очередь заказов
    /// </summary>
    public sealed record OrderQueueSnapshot
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Тип обновления очереди
        /// </summary>
        public required OrderQueueUpdateType OrderQueueUpdateType { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required OrderPosition[] Positions { get; init; }
    }
}
