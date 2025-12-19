using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    [AsyncApi]
    public sealed class OrderQueueNotificationHub(OrderQueueService orderQueueService) : Hub<IOrderQueueNotificationClient>, IOrderQueueNotificationServer
    {
        public override Task OnConnectedAsync() => GetSnapshot();

        public async Task GetSnapshot()
        {
            OrderQueueSnapshot snapshot = await orderQueueService.GetQueueSnapshot();
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot);

            await Clients.Caller.ReceiveSnapshot(message);
        }
    }
}