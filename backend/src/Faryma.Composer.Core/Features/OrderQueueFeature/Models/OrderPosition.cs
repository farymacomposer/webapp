using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Представляет позицию заказа в очереди, включая сам заказ и историю перемещений
    /// </summary>
    public sealed record OrderPosition
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrder Order { get; set; }

        /// <summary>
        /// История изменений позиции заказа в очереди
        /// </summary>
        public OrderPositionHistory PositionHistory { get; } = new();

        public void Swap() => PositionHistory.Previous.Swap(PositionHistory.Current);
        public void SetCurrentPosition(int index, OrderActivityStatus status) => PositionHistory.Current.Set(index, status);

        public void SetCurrentCategory(OrderCategoryType type, int debtNumber)
        {
            PositionHistory.Current.Category = new OrderCategoryInfo
            {
                Type = type,
                DebtNumber = debtNumber
            };
        }
    }
}