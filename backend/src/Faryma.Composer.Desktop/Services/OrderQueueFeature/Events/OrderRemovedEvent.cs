using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature.Events
{
    /// <summary>
    /// Заказ удален
    /// </summary>
    public sealed record OrderRemovedEvent
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }
    }
}