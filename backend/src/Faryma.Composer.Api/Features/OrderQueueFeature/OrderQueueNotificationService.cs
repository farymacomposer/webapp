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
        [Channel("order-queue/new-order", Servers = new[] { Globals.SignalrApiServer })]
        [SubscribeOperation(typeof(NewOrderAddedEvent), OperationId = "NewOrderAdded", Summary = "Добавлен новый заказ")]
        public async Task NotifyNewOrderAdded(OrderPosition orderPosition) =>
            await context.Clients.All.SendAsync("NewOrderAdded", NewOrderAddedEvent.Map(orderPosition));

        [Channel("order-queue/order-removed", Servers = new[] { Globals.SignalrApiServer })]
        [SubscribeOperation(typeof(OrderRemovedEvent), OperationId = "OrderRemoved", Summary = "Заказ удален")]
        public async Task NotifyOrderRemoved(OrderPosition orderPosition) =>
            await context.Clients.All.SendAsync("OrderRemoved", OrderRemovedEvent.Map(orderPosition));

        [Channel("order-queue/position-changed", Servers = new[] { Globals.SignalrApiServer })]
        [SubscribeOperation(typeof(OrderPositionChangedEvent), OperationId = "OrderPositionChanged", Summary = "Изменена позиция заказа")]
        public async Task NotifyOrderPositionChanged(OrderPosition orderPosition) =>
            await context.Clients.All.SendAsync("OrderPositionChanged", OrderPositionChangedEvent.Map(orderPosition));
    }
}