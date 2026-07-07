using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Api.Contracts.Features.OrderQueue.Dto
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
        /// Категория заказа в очереди
        /// </summary>
        public required QueueCategory QueueCategory { get; init; }

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
                QueueCategory = item.Category.QueueCategory,
                CategoryDebtIndex = item.Category.DebtIndex,
            };
        }
    }
}
