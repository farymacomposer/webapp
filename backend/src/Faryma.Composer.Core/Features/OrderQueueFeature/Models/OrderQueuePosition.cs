using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Models
{
    /// <summary>
    /// Позиция заказа в очереди, включая его индекс, статус активности и категорию
    /// </summary>
    public sealed class OrderQueuePosition
    {
        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public int QueueIndex { get; private set; }

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public OrderActivityStatus ActivityStatus { get; private set; }

        /// <summary>
        /// Категория заказа
        /// </summary>
        public OrderCategoryInfo Category { get; set; } = new();

        /// <summary>
        /// Копирует состояние из другой позиции в текущую
        /// </summary>
        public void CopyFrom(OrderQueuePosition current)
        {
            QueueIndex = current.QueueIndex;
            ActivityStatus = current.ActivityStatus;
            Category = current.Category;
        }

        /// <summary>
        /// Обновляет позицию заказа в очереди и статус активности
        /// </summary>
        public void UpdatePosition(int index, OrderActivityStatus status)
        {
            QueueIndex = index;
            ActivityStatus = status;
        }

        public OrderQueuePosition Clone()
        {
            return new()
            {
                QueueIndex = QueueIndex,
                ActivityStatus = ActivityStatus,
                Category = Category,
            };
        }
    }
}