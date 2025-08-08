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

        [Channel("NewOrderAdded", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(NewOrderAddedEvent))]
        public Task NotifyNewOrderAdded(OrderPosition orderPosition) =>
            context.Clients.All.SendAsync("NewOrderAdded", NewOrderAddedEvent.Map(orderPosition));

        [Channel("OrderRemoved", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(OrderRemovedEvent))]
        public Task NotifyOrderRemoved(OrderPosition orderPosition) =>
            context.Clients.All.SendAsync("OrderRemoved", OrderRemovedEvent.Map(orderPosition));

        [Channel("OrderPositionChanged", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(OrderPositionChangedEvent))]
        public Task NotifyOrderPositionChanged(OrderPosition orderPosition) =>
            context.Clients.All.SendAsync("OrderPositionChanged", OrderPositionChangedEvent.Map(orderPosition));
    }
}