using Faryma.Composer.Api.Contracts.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Api.Features.ReviewOrder;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Microsoft.AspNetCore.SignalR;

namespace Faryma.Composer.Api.Features.OrderQueue
{
    public sealed class OrderQueueNotificationService(
        IHubContext<OrderQueueNotificationHub, IClient> context,
        IServiceScopeFactory scopeFactory) : IOrderQueueNotificationService
    {
        public async Task NotifyQueueUpdated(OrderQueueSnapshot snapshot)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            ReviewOrderDtoMapper reviewOrderDtoMapper = scope.ServiceProvider.GetRequiredService<ReviewOrderDtoMapper>();
            OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(snapshot, reviewOrderDtoMapper.Map);

            await context.Clients.All.ReceiveSnapshot(message);
        }
    }
}
