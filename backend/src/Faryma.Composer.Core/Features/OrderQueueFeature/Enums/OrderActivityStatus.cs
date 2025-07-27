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
        Future = 1,

        /// <summary>
        /// Заказ активен
        /// </summary>
        Active = 2,

        /// <summary>
        /// Заказ неактивен
        /// </summary>
        Inactive = 3
    }
}