namespace Faryma.Composer.Core.Features.OrderQueueFeature.Enums
{
    /// <summary>
    /// Тип категории заказа
    /// </summary>
    public enum OrderCategoryType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Заказ обрабатывается вне очереди
        /// </summary>
        OutOfQueue = 1,

        /// <summary>
        /// Заказ является донатом (имеет приоритет над долговой категорией)
        /// </summary>
        Donation = 2,

        /// <summary>
        /// Заказ относится к долговой категории
        /// </summary>
        Debt = 3,
    }
}