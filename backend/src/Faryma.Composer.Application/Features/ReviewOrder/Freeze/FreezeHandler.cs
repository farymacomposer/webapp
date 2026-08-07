using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Freeze
{
    public sealed class FreezeHandler(
        UnitOfWork uow,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<FreezeCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(FreezeCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await uow.ReviewOrderStore.Get(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.IsFrozen)
            {
                return order;
            }

            order.Freeze();

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderFrozen, previousStatus);

            return order;
        }
    }
}
