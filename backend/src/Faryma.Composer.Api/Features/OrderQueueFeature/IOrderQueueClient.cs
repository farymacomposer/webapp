using Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public interface IOrderQueueClient
    {
        [Channel(nameof(ReceiveQueueSnapshot), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueSnapshotMessage),
            Summary = "Передача полного снимка очереди",
            Description = "Отправляется при подключении клиента или по запросу")]
        Task ReceiveQueueSnapshot(OrderQueueSnapshotMessage message);

        [Channel(nameof(ReceiveQueueUpdated), Servers = new[] { OrderQueueNotificationHub.HubServerName })]
        [PublishOperation(
            typeof(OrderQueueUpdatedEvent),
            Summary = "Инкрементальные обновления очереди",
            Description = "Отправляется при каждом изменении очереди")]
        Task ReceiveQueueUpdated(OrderQueueUpdatedEvent @event);
    }
}