using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Unfreeze
{
    public sealed class UnfreezeHandler(
        ReviewOrderStore reviewOrderStore,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<UnfreezeCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(UnfreezeCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (!order.IsFrozen)
            {
                return order;
            }

            order.Unfreeze();

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderUnfrozen, previousStatus);

            return order;
        }
    }
}
