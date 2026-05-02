using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    [AsyncApi]
    public sealed class OrderQueueNotificationHub(OrderQueueService orderQueueService) : Hub<IClient>, IOrderQueueNotificationServer
    {
        public override async Task OnConnectedAsync()
        {
            await GetSnapshot();
            await base.OnConnectedAsync();
        }

        [Channel(nameof(GetSnapshot), Servers = new[] { IOrderQueueNotificationServer.HubServerName })]
        [SubscribeOperation(typeof(object), Description = "Запрос полного снимка очереди")]
        public async Task GetSnapshot()
        {
            OrderQueueSnapshot snapshot = await orderQueueService.GetQueueSnapshot();
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot);

            await Clients.Caller.ReceiveSnapshot(message);
        }
    }
}
