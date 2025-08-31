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
        /// Добавление заказа
        /// </summary>
        Add = 1,

        /// <summary>
        /// Поднятие заказа в очереди
        /// </summary>
        Up = 2,

        /// <summary>
        /// Добавление/изменение ссылки на трек в заказе
        /// </summary>
        AddTrackUrl = 3,

        /// <summary>
        /// Взятие заказа в работу
        /// </summary>
        TakeInProgress = 4,

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        Complete = 5,

        /// <summary>
        /// Заморозка заказа
        /// </summary>
        Freeze = 6,

        /// <summary>
        /// Разморозка заказа
        /// </summary>
        Unfreeze = 7,

        /// <summary>
        /// Отмена заказа
        /// </summary>
        Cancel = 8,

        /// <summary>
        /// Стрим запущен
        /// </summary>
        StreamStarted = 9,

        /// <summary>
        /// Стрим завершен
        /// </summary>
        StreamCompleted = 10,
    }
}