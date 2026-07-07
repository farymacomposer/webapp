using System.ComponentModel;

namespace Faryma.Composer.Domain.Enums
{
    /// <summary>
    /// Заказ или услуга, которую может покрывать пользовательское право
    /// </summary>
    public enum UserEntitlementTarget
    {
        /// <summary>
        /// Не задано
        /// </summary>
        [Description("Не задано")]
        Unspecified = 0,

        /// <summary>
        /// Заказ разбора трека вне очереди
        /// </summary>
        [Description("Заказ разбора трека вне очереди")]
        OutOfQueueReviewOrder = 1,

        /// <summary>
        /// Бесплатный заказ разбора трека
        /// </summary>
        [Description("Бесплатный заказ разбора трека")]
        FreeReviewOrder = 2,

        /// <summary>
        /// Услуга подробного разбора трека
        /// </summary>
        [Description("Услуга подробного разбора трека")]
        DetailedReview = 3,
    }
}
