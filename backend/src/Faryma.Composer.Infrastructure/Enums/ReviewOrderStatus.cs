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
        /// Трек ожидает разбора
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Трек в процессе разбора
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Разбор трека выполнен
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Заказ отменен
        /// </summary>
        Canceled = 5
    }
}