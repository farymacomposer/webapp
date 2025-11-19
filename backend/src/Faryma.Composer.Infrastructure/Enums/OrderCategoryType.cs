using System.ComponentModel;

namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Тип категории заказа
    /// </summary>
    public enum OrderCategoryType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Заказ обрабатывается вне очереди
        /// </summary>
        [Description("Вне очереди")]
        OutOfQueue = 1,

        /// <summary>
        /// Заказ является донатом (имеет приоритет над долговой категорией)
        /// </summary>
        [Description("Донат")]
        Donation = 2,

        /// <summary>
        /// Заказ относится к долговой категории
        /// </summary>
        [Description("Долг")]
        Debt = 3,
    }
}