using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Freeze
{
    public sealed class FreezeHandler(
        ReviewOrderStore reviewOrderStore,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<FreezeCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(FreezeCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.IsFrozen)
            {
                return order;
            }

            order.Freeze();

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderFrozen, previousStatus);

            return order;
        }
    }
}
