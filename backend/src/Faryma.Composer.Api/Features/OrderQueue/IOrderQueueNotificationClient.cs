using Faryma.Composer.Api.Contracts.Features.OrderQueue.AsyncContracts;

namespace Faryma.Composer.Api.Contracts.Features.OrderQueue
{
    public interface IOrderQueueNotificationClient
    {
        /// <summary>
        /// Передача полного снимка очереди
        /// </summary>
        Task ReceiveSnapshot(OrderQueueSnapshotMessage message);
    }
}
