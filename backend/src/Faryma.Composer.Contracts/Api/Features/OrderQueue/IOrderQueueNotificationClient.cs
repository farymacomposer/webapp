using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue
{
    public interface IOrderQueueNotificationClient
    {
        /// <summary>
        /// Передача полного снимка очереди
        /// </summary>
        Task ReceiveSnapshot(OrderQueueSnapshotMessage message);

        /// <summary>
        /// Инкрементальные обновления очереди
        /// </summary>
        Task ReceiveUpdated(OrderQueueUpdatedEvent @event);
    }
}