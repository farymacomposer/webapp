using System.ComponentModel;

namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Тип заказа разбора трека
    /// </summary>
    public enum ReviewOrderType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Вне очереди
        /// </summary>
        [Description("Вне очереди")]
        OutOfQueue = 1,

        /// <summary>
        /// Донат
        /// </summary>
        [Description("Донат")]
        Donation = 2,

        /// <summary>
        /// Бесплатный
        /// </summary>
        [Description("Бесплатный")]
        Free = 3,

        /// <summary>
        /// Благотворительный
        /// </summary>
        [Description("Благотворительный")]
        Charity = 4,

        /// <summary>
        /// Заказной (индивидуальный заказ)
        /// </summary>
        [Description("Заказной")]
        Custom = 5
    }
}