using System.ComponentModel;

namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Статус заказа разбора трека
    /// </summary>
    public enum ReviewOrderStatus
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Предзаказ
        /// </summary>
        [Description("Предзаказ")]
        Preorder = 1,

        /// <summary>
        /// Ожидает взятия в работу
        /// </summary>
        [Description("Ожидает взятия в работу")]
        Pending = 2,

        /// <summary>
        /// В работе
        /// </summary>
        [Description("В работе")]
        InProgress = 3,

        /// <summary>
        /// Выполнен
        /// </summary>
        [Description("Выполнен")]
        Completed = 4,

        /// <summary>
        /// Отменен
        /// </summary>
        [Description("Отменен")]
        Canceled = 5
    }
}