using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    /// <summary>
    /// Позиция заказа в очереди, включая его индекс, статус активности и категорию
    /// </summary>
    public sealed record OrderQueuePositionDto
    {
        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public required int QueueIndex { get; init; }

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public required OrderActivityStatus ActivityStatus { get; init; }

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public required OrderCategoryType CategoryType { get; init; }

        /// <summary>
        /// Индекс категории, если заказ относится к долговой категории
        /// </summary>
        public required int CategoryDebtIndex { get; init; }

        public static OrderQueuePositionDto Map(OrderQueuePosition item)
        {
            return new()
            {
                QueueIndex = item.QueueIndex,
                ActivityStatus = item.ActivityStatus,
                CategoryType = item.Category.Type,
                CategoryDebtIndex = item.Category.DebtIndex,
            };
        }
    }
}