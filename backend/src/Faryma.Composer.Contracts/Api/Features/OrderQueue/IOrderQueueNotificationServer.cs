using Saunter.Attributes;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue
{
    public interface IOrderQueueNotificationServer
    {
        public const string HubServerName = "OrderQueueNotificationHub";
        public const string RoutePattern = $"/api/{HubServerName}";

        [Channel(nameof(GetSnapshot), Servers = new[] { HubServerName })]
        [SubscribeOperation(
            typeof(object),
            Description = "Запрос полного снимка очереди")]
        Task GetSnapshot();
    }
}