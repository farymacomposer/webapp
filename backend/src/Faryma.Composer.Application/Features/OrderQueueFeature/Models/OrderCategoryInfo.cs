using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Содержит информацию о категории заказа, включая тип категории и номер (для долговых категорий)
    /// </summary>
    public sealed record OrderCategoryInfo
    {
        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public required OrderCategoryType Type { get; init; }

        /// <summary>
        /// Индекс категории, если заказ относится к долговой категории
        /// </summary>
        public required int DebtIndex { get; init; }
    }
}