namespace Faryma.Composer.Desktop.Api.OrderQueue.Dto
{
    /// <summary>
    /// Ответ на запрос получения очереди заказов
    /// </summary>
    public sealed record OrderQueueSnapshotMessage
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Активные заказы
        /// </summary>
        public required IEnumerable<OrderPositionDto> ActiveOrders { get; init; }

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public required OrderPositionDto? InProgressOrder { get; init; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        public required IEnumerable<OrderPositionDto> CompletedOrders { get; init; }

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        public required IEnumerable<OrderPositionDto> ScheduledOrders { get; init; }

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        public required IEnumerable<OrderPositionDto> FrozenOrders { get; init; }
    }
}