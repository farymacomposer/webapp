using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl
{
    public sealed class AddTrackUrlHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<AddTrackUrlCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(AddTrackUrlCommand command, CancellationToken ct = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.TrackDurationSeconds);

            ReviewOrderEntity order = await reviewOrderService.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(command.TrackDurationSeconds);

            order.AddTrackUrl(command.TrackUrl, command.TrackDurationSeconds, requiredAmount);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.TrackUrlAdded, previousStatus));

            return order;
        }
    }
}
