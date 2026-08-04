using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Unfreeze
{
    public sealed class UnfreezeHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<UnfreezeCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(UnfreezeCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderService.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (!order.IsFrozen)
            {
                return order;
            }

            order.Unfreeze();

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderUnfrozen, previousStatus));

            return order;
        }
    }
}
