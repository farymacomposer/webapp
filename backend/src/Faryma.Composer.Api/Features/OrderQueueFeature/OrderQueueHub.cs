using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed class OrderQueueHub : Hub, INotificationService
    {
        public async Task SendOrderPosition(OrderPositionTracker position) => await Clients.All.SendAsync("ReceiveOrderPosition", position);
    }
}