using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl
{
    public sealed class AddTrackUrlHandler(
        ReviewOrderStore reviewOrderStore,
        ReviewOrderService reviewOrderService,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<AddTrackUrlCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(AddTrackUrlCommand command, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(command.TrackDurationSeconds);

            order.AddTrackUrl(command.TrackUrl, command.TrackDurationSeconds, requiredAmount);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.TrackUrlAdded, previousStatus);

            return order;
        }
    }
}
