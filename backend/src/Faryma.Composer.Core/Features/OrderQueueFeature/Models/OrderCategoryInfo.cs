using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Содержит информацию о категории заказа, включая тип категории и номер (для долговых категорий)
    /// </summary>
    public sealed record OrderCategoryInfo
    {
        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType Type { get; init; }

        /// <summary>
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public int DebtNumber { get; init; }
    }
}