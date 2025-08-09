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

        /// <summary>
        /// Записывает текущее состояние в предыдущее
        /// </summary>
        public void SaveCurrentPositionToPrevious() => PositionHistory.Previous.CopyFrom(PositionHistory.Current);

        /// <summary>
        /// Обновляет текущую позицию заказа в очереди
        /// </summary>
        public void UpdateCurrentPosition(int index, OrderActivityStatus status) => PositionHistory.Current.UpdatePosition(index, status);

        /// <summary>
        /// Обновляет текущую категорию заказа
        /// </summary>
        public void UpdateCurrentCategory(OrderCategoryType type, int debtNumber)
        {
            PositionHistory.Current.Category = new OrderCategoryInfo
            {
                Type = type,
                DebtNumber = debtNumber
            };
        }
    }
}