using Faryma.Composer.Api.Features.OrderQueue.AsyncContracts;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    public interface IOrderQueueNotificationClient
    {
        /// <summary>
        /// Передача полного снимка очереди
        /// </summary>
        Task ReceiveSnapshot(OrderQueueSnapshotMessage message);
    }
}
