namespace Faryma.Composer.Core.Features.OrderQueueFeature.Enums
{
    /// <summary>
    /// Тип категории заказа
    /// </summary>
    public enum OrderCategoryType
    {
        /// <summary>
        /// Категория не задана
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Вне очереди
        /// </summary>
        OutOfQueue = 1,

        /// <summary>
        /// Донат
        /// </summary>
        Donation = 2,

        /// <summary>
        /// Долг
        /// </summary>
        Debt = 3,
    }
}