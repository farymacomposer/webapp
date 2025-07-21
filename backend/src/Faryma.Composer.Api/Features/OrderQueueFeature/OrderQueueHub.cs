using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueHub : Hub
    {
        public async Task SendOrderPosition(OrderQueuePositionDto position) => await Clients.All.SendAsync("ReceiveOrderPosition", position);
    }
}