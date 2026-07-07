using System.ComponentModel;

namespace Faryma.Composer.Domain.Enums
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
        /// Ожидает оплаты
        /// </summary>
        [Description("Ожидает оплаты")]
        AwaitingPayment = 2,

        /// <summary>
        /// Ожидает взятия в работу
        /// </summary>
        [Description("Ожидает взятия в работу")]
        Pending = 3,

        /// <summary>
        /// В работе
        /// </summary>
        [Description("В работе")]
        InProgress = 4,

        /// <summary>
        /// Выполнен
        /// </summary>
        [Description("Выполнен")]
        Completed = 5,

        /// <summary>
        /// Отменен
        /// </summary>
        [Description("Отменен")]
        Canceled = 6,
    }
}
