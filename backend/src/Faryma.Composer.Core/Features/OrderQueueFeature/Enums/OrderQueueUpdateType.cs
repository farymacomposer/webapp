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
        /// Поднятие заказа в очереди
        /// </summary>
        Up = 1,

        /// <summary>
        /// Добавление/изменение ссылки на трек в заказе
        /// </summary>
        AddTrackUrl = 2,

        /// <summary>
        /// Взятие заказа в работу
        /// </summary>
        TakeInProgress = 3,

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        Complete = 4,

        /// <summary>
        /// Заморозка заказа
        /// </summary>
        Freeze = 5,

        /// <summary>
        /// Разморозка заказа
        /// </summary>
        Unfreeze = 6,
    }
}