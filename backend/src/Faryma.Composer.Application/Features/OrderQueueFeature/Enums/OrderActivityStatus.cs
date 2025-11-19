using System.ComponentModel;

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
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Заказ активен
        /// </summary>
        [Description("Заказ активен")]
        Active = 1,

        /// <summary>
        /// Заказ в процессе выполнения
        /// </summary>
        [Description("Заказ в процессе выполнения")]
        InProgress = 2,

        /// <summary>
        /// Заказ выполнен
        /// </summary>
        [Description("Заказ выполнен")]
        Completed = 3,

        /// <summary>
        /// Заказ запланирован на будущий стрим
        /// </summary>
        [Description("Заказ запланирован на будущий стрим")]
        Scheduled = 4,

        /// <summary>
        /// Заказ заморожен и не будет обрабатываться
        /// </summary>
        [Description("Заказ заморожен и не будет обрабатываться")]
        Frozen = 5,

        /// <summary>
        /// Заказ удален из очереди
        /// </summary>
        [Description("Заказ удален из очереди")]
        Removed = 6,
    }
}