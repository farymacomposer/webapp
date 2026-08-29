using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models
{
    /// <summary>
    /// Содержит информацию о категории заказа, включая тип категории и номер (для долговых категорий)
    /// </summary>
    public sealed record OrderCategoryInfo
    {
        /// <summary>
        /// Категория заказа в очереди
        /// </summary>
        public required QueueCategory QueueCategory { get; init; }

        /// <summary>
        /// Индекс категории, если заказ относится к долговой категории
        /// </summary>
        public required int DebtIndex { get; init; }
    }
}
