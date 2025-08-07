using Faryma.Composer.Api.Constants;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueNotificationHub : Hub;

    [AsyncApi]
    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub> context) : IOrderQueueNotificationService
    {
        [Channel("order-queue/new-order", Servers = new[] { SignalRConstants.SignalRHubAsyncApiServer })]
        [SubscribeOperation(typeof(NewOrderAddedMessage), Summary = "Уведомление о новом заказе")]
        public async Task NotifyNewOrderAdded(OrderPosition orderPosition)
        {
            NewOrderAddedMessage message = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };
            await context.Clients.All.SendAsync("NewOrderAdded", message);
        }

        [Channel("order-queue/order-removed", Servers = new[] { SignalRConstants.SignalRHubAsyncApiServer })]
        [SubscribeOperation(typeof(OrderRemovedMessage), Summary = "Уведомление об удалении заказа")]
        public async Task NotifyOrderRemoved(OrderPosition orderPosition)
        {
            OrderRemovedMessage message = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };
            await context.Clients.All.SendAsync("OrderRemoved", message);
        }

        [Channel("order-queue/position-changed", Servers = new[] { SignalRConstants.SignalRHubAsyncApiServer })]
        [SubscribeOperation(typeof(OrderPositionChangedMessage), Summary = "Уведомление об изменении позиции заказа")]
        public async Task NotifyOrderPositionChanged(OrderPosition orderPosition)
        {
            OrderPositionChangedMessage message = new()
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };
            await context.Clients.All.SendAsync("OrderPositionChanged", message);
        }
    }
}