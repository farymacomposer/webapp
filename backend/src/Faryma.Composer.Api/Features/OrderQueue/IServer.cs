using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    public interface IServer : IOrderQueueNotificationServer
    {
        [Channel(nameof(GetSnapshot), Servers = new[] { HubServerName })]
        [SubscribeOperation(
            typeof(object),
            Description = "Запрос полного снимка очереди")]
        new Task GetSnapshot();
    }
}