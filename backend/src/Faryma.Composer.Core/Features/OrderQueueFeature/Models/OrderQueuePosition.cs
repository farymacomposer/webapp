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

        public void Swap(OrderQueuePosition current)
        {
            QueueIndex = current.QueueIndex;
            ActivityStatus = current.ActivityStatus;
            Category = current.Category;
        }

        public void Set(int index, OrderActivityStatus status)
        {
            QueueIndex = index;
            ActivityStatus = status;
        }
    }
}