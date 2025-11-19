using Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts;
using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    [AsyncApi]
    public sealed class OrderQueueNotificationHub(OrderQueueService orderQueueService) : Hub<IOrderQueueNotificationClient>
    {
        public const string HubServerName = nameof(OrderQueueNotificationHub);
        public const string RoutePattern = $"/api/{HubServerName}";

        public override Task OnConnectedAsync() => GetSnapshot();

        [Channel(nameof(GetSnapshot), Servers = new[] { HubServerName })]
        [SubscribeOperation(
            typeof(object),
            Summary = "Запрос полного снимка очереди",
            Description = "Позволяет клиенту синхронизировать очередь по требованию")]
        public async Task GetSnapshot()
        {
            OrderQueueSnapshot snapshot = await orderQueueService.GetQueueSnapshot();
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot);

            await Clients.Caller.ReceiveSnapshot(message);
        }
    }
}