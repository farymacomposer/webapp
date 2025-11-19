using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Desktop.Api.OrderQueue.Dto
{
    /// <summary>
    /// Изменены позиции заказов
    /// </summary>
    public sealed record OrderQueueUpdatedEvent
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
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }
    }
}