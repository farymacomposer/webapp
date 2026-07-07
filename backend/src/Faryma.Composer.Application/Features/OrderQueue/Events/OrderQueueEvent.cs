using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.Events
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
