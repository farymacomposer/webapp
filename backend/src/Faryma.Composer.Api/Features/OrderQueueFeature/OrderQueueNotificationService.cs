using Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts;
using Faryma.Composer.Application.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;
using Faryma.Composer.Contracts.Api.Features.OrderQueueFeature;
using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub, IOrderQueueNotificationClient> context) : IOrderQueueNotificationService
    {
        public async Task NotifyQueueUpdated(OrderQueueSnapshot snapshot)
        {
            OrderQueueUpdatedEvent @event = OrderQueueUpdatedEvent.Map(snapshot);
            await context.Clients.All.ReceiveUpdated(@event);
        }
    }
}