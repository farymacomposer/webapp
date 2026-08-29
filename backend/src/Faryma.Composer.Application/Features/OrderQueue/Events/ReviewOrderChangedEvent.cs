using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.Events
{
    /// <summary>
    /// Заказ был изменен
    /// </summary>
    public sealed class ReviewOrderChangedEvent(ReviewOrderEntity order, OrderQueueUpdateType updateType, ReviewOrderStatus previousStatus) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity Order { get; } = order;

        /// <summary>
        /// Предыдущий статус заказа
        /// </summary>
        public ReviewOrderStatus PreviousStatus { get; } = previousStatus;
    }
}
