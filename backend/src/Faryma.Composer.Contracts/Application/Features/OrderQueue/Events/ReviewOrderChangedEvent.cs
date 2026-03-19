using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Application.Features.OrderQueue.Events
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