using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueNotificationHub : Hub;

    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub> context) : IOrderQueueNotificationService
    {
        public async Task NotifyNewOrderAdded(OrderPosition orderPosition)
        {
            await context.Clients.All.SendAsync("NewOrderAdded", new
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
            });
        }

        public async Task NotifyOrderRemoved(OrderPosition orderPosition)
        {
            await context.Clients.All.SendAsync("OrderRemoved", new
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
            });
        }

        public async Task NotifyOrderPositionChanged(OrderPosition orderPosition)
        {
            await context.Clients.All.SendAsync("OrderPositionChanged", new
            {
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous),
            });
        }

        public async Task NotifyReviewStarted(ReviewOrder reviewOrder)
        {
            await context.Clients.All.SendAsync("ReviewStarted", new
            {
                Order = ReviewOrderDto.Map(reviewOrder)
            });
        }

        public async Task NotifyReviewCompleted(ReviewOrder reviewOrder)
        {
            await context.Clients.All.SendAsync("ReviewCompleted", new
            {
                Order = ReviewOrderDto.Map(reviewOrder)
            });
        }
    }
}