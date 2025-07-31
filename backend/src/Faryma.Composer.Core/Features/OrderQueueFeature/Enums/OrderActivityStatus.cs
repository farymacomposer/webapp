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
        /// Заказ запланирован на будущий стрим
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Заказ активен
        /// </summary>
        Active = 2,

        /// <summary>
        /// Заказ заморожен и не будет обрабатываться
        /// </summary>
        Frozen = 3
    }
}