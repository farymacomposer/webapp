using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Категория заказа
    /// </summary>
    public sealed record OrderCategoryInfo
    {
        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType Type { get; init; }

        /// <summary>
        /// Номер долговой категории
        /// </summary>
        public int DebtNumber { get; init; }
    }
}