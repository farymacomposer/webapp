using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Saunter.Attributes;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue
{
    public interface IOrderQueueNotificationClient
    {
        [Channel(nameof(ReceiveSnapshot), Servers = new[] { IOrderQueueNotificationServer.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueSnapshotMessage),
            Description = "Передача полного снимка очереди")]
        Task ReceiveSnapshot(OrderQueueSnapshotMessage message);

        [Channel(nameof(ReceiveUpdated), Servers = new[] { IOrderQueueNotificationServer.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueUpdatedEvent),
            Description = "Инкрементальные обновления очереди")]
        Task ReceiveUpdated(OrderQueueUpdatedEvent @event);
    }
}