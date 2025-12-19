using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Представляет позицию заказа в очереди, включая сам заказ и историю перемещений
    /// </summary>
    public sealed class OrderPosition
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity Order { get; private set; } = null!;

        /// <summary>
        /// История изменений позиции заказа в очереди
        /// </summary>
        public required OrderPositionHistory PositionHistory { get; init; }

        /// <summary>
        /// Признак обновления заказа
        /// </summary>
        public bool IsOrderUpdated { get; private set; }

        public static OrderPosition Create(ReviewOrderEntity order)
        {
            return new()
            {
                Order = order,
                IsOrderUpdated = true,
                PositionHistory = new OrderPositionHistory
                {
                    Current = new OrderQueuePosition
                    {
                        Category = new OrderCategoryInfo
                        {
                            Type = OrderCategoryType.Unspecified,
                            DebtIndex = 0
                        }
                    },
                    Previous = new OrderQueuePosition
                    {
                        Category = new OrderCategoryInfo
                        {
                            Type = OrderCategoryType.Unspecified,
                            DebtIndex = 0
                        }
                    }
                }
            };
        }

        public void UpdateOrder(ReviewOrderEntity order)
        {
            Order = order;
            IsOrderUpdated = true;
        }

        /// <summary>
        /// Записывает текущее состояние в предыдущее
        /// </summary>
        public void SaveCurrentPositionToPrevious()
        {
            PositionHistory.Previous.CopyFrom(PositionHistory.Current);
            IsOrderUpdated = false;
        }

        /// <summary>
        /// Обновляет текущую позицию заказа в очереди
        /// </summary>
        public void UpdateCurrentPosition(int index, OrderActivityStatus status) => PositionHistory.Current.UpdatePosition(index, status);

        /// <summary>
        /// Обновляет текущую категорию заказа
        /// </summary>
        public void UpdateCurrentCategory(OrderCategoryType type, int debtIndex)
        {
            PositionHistory.Current.Category = new OrderCategoryInfo
            {
                Type = type,
                DebtIndex = debtIndex
            };
        }

        public OrderPosition Clone()
        {
            return new()
            {
                Order = Order,
                PositionHistory = PositionHistory.Clone(),
            };
        }
    }
}