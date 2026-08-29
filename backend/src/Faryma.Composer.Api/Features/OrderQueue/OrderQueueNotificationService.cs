using Faryma.Composer.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Models;
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
