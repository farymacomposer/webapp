namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Статус заказа разбора трека
    /// </summary>
    public enum ReviewOrderStatus
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Предзаказ
        /// </summary>
        Preorder = 1,

        /// <summary>
        /// Ожидает взятия в работу
        /// </summary>
        Pending = 2,

        /// <summary>
        /// В работе
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Выполнен
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Отменен
        /// </summary>
        Canceled = 5
    }
}