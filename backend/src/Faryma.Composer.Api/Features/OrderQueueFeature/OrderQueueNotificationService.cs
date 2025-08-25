using Faryma.Composer.Api.Features.OrderQueueFeature.Events;
using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
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

        [Channel("NewOrderAdded", Servers = new[] { HubServerName })]
        [SubscribeOperation(typeof(NewOrderAddedEvent), Description = "Уведомляет о создании нового заказа")]
        public async Task NotifyNewOrderAdded(int syncVersion, OrderPosition position)
        {
            NewOrderAddedEvent item = NewOrderAddedEvent.Map(syncVersion, position);
            await context.Clients.All.SendAsync("NewOrderAdded", item);
        }

        [Channel("OrderPositionChanged", Servers = new[] { HubServerName })]
        [SubscribeOperation(typeof(OrderPositionChangedEvent), Description = "Уведомляет об изменении позиции заказа")]
        public async Task NotifyOrderPositionChanged(int syncVersion, OrderPosition position, OrderQueueUpdateType updateType)
        {
            OrderPositionChangedEvent item = OrderPositionChangedEvent.Map(syncVersion, position, updateType);
            await context.Clients.All.SendAsync("OrderPositionChanged", item);
        }

        [Channel("OrderPositionsChanged", Servers = new[] { HubServerName })]
        [SubscribeOperation(typeof(OrderPositionsChangedEvent), Description = "Уведомляет об изменении позиций заказов")]
        public async Task NotifyOrderPositionsChanged(OrderQueue orderQueue)
        {
            OrderPositionsChangedEvent item = OrderPositionsChangedEvent.Map(orderQueue);
            await context.Clients.All.SendAsync("OrderPositionsChanged", item);
        }

        [Channel("OrderRemoved", Servers = new[] { HubServerName })]
        [SubscribeOperation(typeof(OrderRemovedEvent), Description = "Уведомляет об удалении заказа")]
        public async Task NotifyOrderRemoved(int syncVersion, OrderPosition position)
        {
            OrderRemovedEvent item = OrderRemovedEvent.Map(syncVersion, position);
            await context.Clients.All.SendAsync("OrderRemoved", item);
        }
    }
}