using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Cancel
{
    public sealed class CancelHandler(
        ReviewOrderStore reviewOrderStore,
        DateTimeService dateTimeService,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<CancelCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CancelCommand command, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            order.Cancel(command.CancelReason, dateTimeService.Now);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCanceled, previousStatus);

            return order;
        }
    }
}
