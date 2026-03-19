using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    public sealed class OrderQueueNotificationService(IHubContext<OrderQueueNotificationHub, IClient> context) : IOrderQueueNotificationService
    {
        public async Task NotifyQueueUpdated(OrderQueueSnapshot snapshot)
        {
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot);
            await context.Clients.All.ReceiveSnapshot(message);
        }
    }
}