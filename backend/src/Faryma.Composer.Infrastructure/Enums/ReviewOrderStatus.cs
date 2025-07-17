namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Статус заказа разбора трека
    /// </summary>
    public enum ReviewOrderStatus
    {
        /// <summary>
        /// Предзаказ
        /// </summary>
        Preorder = 0,

        /// <summary>
        /// Трек ожидает разбора
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Трек в процессе разбора
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Разбор трека выполнен
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Заказ отменен
        /// </summary>
        Canceled = 4
    }
}