using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Cancel
{
    public sealed class CancelHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        OrderQueueEventChannel orderQueueEventChannel,
        DateTimeService dateTimeService) : IRequestHandler<CancelCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CancelCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderService.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            order.Cancel(command.CancelReason, dateTimeService.Now);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCanceled, previousStatus);

            return order;
        }
    }
}
