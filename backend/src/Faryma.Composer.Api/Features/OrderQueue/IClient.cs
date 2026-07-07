using Faryma.Composer.Api.Contracts.Features.OrderQueue;
using Faryma.Composer.Api.Contracts.Features.OrderQueue.AsyncContracts;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    public interface IClient : IOrderQueueNotificationClient
    {
        [Channel(nameof(ReceiveSnapshot), Servers = new[] { IOrderQueueNotificationServer.HubServerName })]
        [PublishOperation(typeof(OrderQueueSnapshotMessage), Description = "Передача полного снимка очереди")]
        new Task ReceiveSnapshot(OrderQueueSnapshotMessage message);
    }
}
