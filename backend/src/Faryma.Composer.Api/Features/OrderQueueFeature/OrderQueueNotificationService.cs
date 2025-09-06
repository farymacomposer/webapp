using Faryma.Composer.Api.Features.OrderQueueFeature.Events;
using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueNotificationHub : Hub
    {
        public const string RoutePattern = "/api/OrderQueueNotificationHub";
    }

    [AsyncApi]
    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub> context) : IOrderQueueNotificationService
    {
        public const string HubServerName = "OrderQueueNotificationHub";

        [Channel("OrderPositionsChanged", Servers = new[] { HubServerName })]
        [SubscribeOperation(typeof(OrderPositionsChangedEvent), Description = "Уведомляет об изменении позиций заказов")]
        public async Task NotifyOrderPositionsChanged(OrderQueue orderQueue)
        {
            OrderPositionsChangedEvent item = OrderPositionsChangedEvent.Map(orderQueue);
            await context.Clients.All.SendAsync("OrderPositionsChanged", item);
        }
    }
}