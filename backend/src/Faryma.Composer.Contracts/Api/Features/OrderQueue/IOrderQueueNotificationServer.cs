namespace Faryma.Composer.Contracts.Api.Features.OrderQueue
{
    public interface IOrderQueueNotificationServer
    {
        public const string HubServerName = "OrderQueueNotificationHub";
        public const string RoutePattern = $"/api/{HubServerName}";

        /// <summary>
        /// Запрос полного снимка очереди
        /// </summary>
        Task GetSnapshot();
    }
}