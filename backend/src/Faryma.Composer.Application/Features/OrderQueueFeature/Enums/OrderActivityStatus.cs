namespace Faryma.Composer.Application.Features.OrderQueueFeature.Enums
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
        /// Заказ активен
        /// </summary>
        Active = 1,

        /// <summary>
        /// Заказ в процессе выполнения
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Заказ выполнен
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Заказ запланирован на будущий стрим
        /// </summary>
        Scheduled = 4,

        /// <summary>
        /// Заказ заморожен и не будет обрабатываться
        /// </summary>
        Frozen = 5,

        /// <summary>
        /// Заказ удален из очереди
        /// </summary>
        Removed = 6,
    }
}