using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Api.OrderQueue.Dto
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
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public required int CategoryDebtIndex { get; init; }
    }
}