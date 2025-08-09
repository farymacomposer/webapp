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
    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub> context, ILogger<OrderQueueNotificationService> logger) : IOrderQueueNotificationService
    {
        public const string HubServerName = "OrderQueueNotificationHub";

        /// <summary>
        /// Уведомляет о создании нового заказа
        /// </summary>
        [Channel("NewOrderAdded", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(NewOrderAddedEvent))]
        public async Task NotifyNewOrderAdded(OrderPosition position)
        {
            NewOrderAddedEvent item = NewOrderAddedEvent.Map(position);
            logger.LogInformation("NotifyNewOrderAdded {@item}", item);
            await context.Clients.All.SendAsync("NewOrderAdded", item);
        }

        /// <summary>
        /// Уведомляет об изменении позиции заказа
        /// </summary>
        [Channel("OrderPositionChanged", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(OrderPositionChangedEvent))]
        public async Task NotifyOrderPositionChanged(OrderPosition position, OrderQueueUpdateType updateType)
        {
            OrderPositionChangedEvent item = OrderPositionChangedEvent.Map(position, updateType);
            logger.LogInformation("NotifyOrderPositionChanged {@item}", item);
            await context.Clients.All.SendAsync("OrderPositionChanged", item);
        }

        /// <summary>
        /// Уведомляет об удалении заказа
        /// </summary>
        [Channel("OrderRemoved", Servers = new[] { HubServerName })]
        [PublishOperation(typeof(OrderRemovedEvent))]
        public async Task NotifyOrderRemoved(OrderPosition position)
        {
            OrderRemovedEvent item = OrderRemovedEvent.Map(position);
            logger.LogInformation("NotifyOrderRemoved {@item}", item);
            await context.Clients.All.SendAsync("OrderRemoved", item);
        }
    }
}