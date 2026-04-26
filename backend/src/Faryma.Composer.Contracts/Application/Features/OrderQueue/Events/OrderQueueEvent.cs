using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;

namespace Faryma.Composer.Contracts.Application.Features.OrderQueue.Events
{
    /// <summary>
    /// Событие обновления очереди
    /// </summary>
    public abstract class OrderQueueEvent(OrderQueueUpdateType updateType)
    {
        /// <summary>
        /// Тип обновления очереди
        /// </summary>
        public OrderQueueUpdateType UpdateType { get; } = updateType;
    }
}
