namespace Faryma.Composer.Core.Features.OrderQueueFeature.Enums
{
    /// <summary>
    /// Тип обновления очереди
    /// </summary>
    public enum OrderQueueUpdateType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Заказ был добавлен
        /// </summary>
        OrderCreated = 1,

        /// <summary>
        /// Заказ был поднят в очереди
        /// </summary>
        OrderMovedUp = 2,

        /// <summary>
        /// Была добавлена/изменена ссылка на трек в заказе
        /// </summary>
        TrackUrlAdded = 3,

        /// <summary>
        /// Заказ был взят в работу
        /// </summary>
        OrderTaken = 4,

        /// <summary>
        /// Заказ был выполнен
        /// </summary>
        OrderCompleted = 5,

        /// <summary>
        /// Заказ был заморожен
        /// </summary>
        OrderFrozen = 6,

        /// <summary>
        /// Заказ был разморожен
        /// </summary>
        OrderUnfrozen = 7,

        /// <summary>
        /// Заказ был отменен
        /// </summary>
        OrderCanceled = 8,

        /// <summary>
        /// Стрим был запущен
        /// </summary>
        StreamStarted = 9,

        /// <summary>
        /// Стрим был завершен
        /// </summary>
        StreamCompleted = 10,
    }
}