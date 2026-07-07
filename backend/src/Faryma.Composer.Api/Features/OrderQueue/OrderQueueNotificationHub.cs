using Faryma.Composer.Api.Contracts.Features.OrderQueue;
using Faryma.Composer.Api.Contracts.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Api.Features.ReviewOrder;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Application.Features.OrderQueue;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    [AsyncApi]
    public sealed class OrderQueueNotificationHub(
        OrderQueueService orderQueueService,
        ReviewOrderDtoMapper reviewOrderDtoMapper) : Hub<IClient>, IOrderQueueNotificationServer
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
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot, reviewOrderDtoMapper.Map);

            await Clients.Caller.ReceiveSnapshot(message);
        }
    }
}
