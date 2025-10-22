using Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public interface IOrderQueueClient
    {
        [Channel(nameof(ReceiveOrderQueueSnapshot), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueSnapshotMessage),
            Summary = "Передача полного снимка очереди",
            Description = "Отправляется при подключении клиента или по запросу")]
        Task ReceiveOrderQueueSnapshot(OrderQueueSnapshotMessage message);

        [Channel(nameof(ReceiveOrderQueueUpdated), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueUpdatedEvent),
            Summary = "Инкрементальные обновления очереди",
            Description = "Отправляется при каждом изменении очереди")]
        Task ReceiveOrderQueueUpdated(OrderQueueUpdatedEvent @event);
    }
}