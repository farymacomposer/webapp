namespace Faryma.Composer.Core.Features.OrderQueueFeature.Enums
{
    /// <summary>
    /// Статус активности заказа
    /// </summary>
    public enum OrderActivityStatus
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Активен
        /// </summary>
        Active = 1,

        /// <summary>
        /// В работе
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Выполнен
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Запланирован на будущий стрим
        /// </summary>
        Scheduled = 4,

        /// <summary>
        /// Заморожен и не будет обрабатываться
        /// </summary>
        Frozen = 5
    }
}