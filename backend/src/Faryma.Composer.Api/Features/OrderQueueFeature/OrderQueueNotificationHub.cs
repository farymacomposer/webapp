using Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    [AsyncApi]
    public sealed class OrderQueueNotificationHub(OrderQueueService orderQueueService) : Hub<IOrderQueueClient>
    {
        public const string HubServerName = nameof(OrderQueueNotificationHub);
        public const string RoutePattern = $"/api/{HubServerName}";

        public override Task OnConnectedAsync() => GetQueueSnapshot();

        [Channel(nameof(GetQueueSnapshot), Servers = new[] { HubServerName })]
        [SubscribeOperation(
            typeof(object),
            Summary = "Запрос полного снимка очереди",
            Description = "Позволяет клиенту синхронизировать очередь по требованию")]
        public async Task GetQueueSnapshot()
        {
            OrderQueueSnapshot snapshot = await orderQueueService.GetQueueSnapshot();
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot);

            await Clients.Caller.ReceiveQueueSnapshot(message);
        }
    }
}