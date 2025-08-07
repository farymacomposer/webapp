using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
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
        public async Task NotifyNewOrderAdded(OrderPosition orderPosition)
        {
            NewOrderAddedEvent item = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };

            await context.Clients.All.SendAsync("NewOrderAdded", item);
        }

        [Channel("order-queue/order-removed", Servers = new[] { Globals.SignalrApiServer })]
        [SubscribeOperation(typeof(OrderRemovedEvent), OperationId = "OrderRemoved", Summary = "Заказ удален")]
        public async Task NotifyOrderRemoved(OrderPosition orderPosition)
        {
            OrderRemovedEvent item = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };

            await context.Clients.All.SendAsync("OrderRemoved", item);
        }

        [Channel("order-queue/position-changed", Servers = new[] { Globals.SignalrApiServer })]
        [SubscribeOperation(typeof(OrderPositionChangedEvent), OperationId = "OrderPositionChanged", Summary = "Изменена позиция заказа")]
        public async Task NotifyOrderPositionChanged(OrderPosition orderPosition)
        {
            OrderPositionChangedEvent item = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };

            await context.Clients.All.SendAsync("OrderPositionChanged", item);
        }
    }
}