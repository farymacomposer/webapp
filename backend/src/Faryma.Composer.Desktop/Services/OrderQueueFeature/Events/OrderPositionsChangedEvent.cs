using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature.Events
{
    /// <summary>
    /// Изменены позиции заказов
    /// </summary>
    public sealed record OrderPositionsChangedEvent
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }
    }
}