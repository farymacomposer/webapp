using Saunter.Attributes;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueueFeature
{
    public interface IOrderQueueNotificationClient
    {
        [Channel(nameof(ReceiveSnapshot), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueSnapshotMessage),
            Summary = "Передача полного снимка очереди",
            Description = "Отправляется при подключении клиента или по запросу")]
        Task ReceiveSnapshot(OrderQueueSnapshotMessage message);

        [Channel(nameof(ReceiveUpdated), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueUpdatedEvent),
            Summary = "Инкрементальные обновления очереди",
            Description = "Отправляется при каждом изменении очереди")]
        Task ReceiveUpdated(OrderQueueUpdatedEvent @event);
    }
}