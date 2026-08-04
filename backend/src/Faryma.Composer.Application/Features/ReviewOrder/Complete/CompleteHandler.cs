using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Complete
{
    public sealed class CompleteHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        OrderQueueEventChannel orderQueueEventChannel,
        DateTimeService dateTimeService) : IRequestHandler<CompleteCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CompleteCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderService.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            ReviewEntity review = uow.ReviewStore.Create(order, command.Rating, createdByUser);

            order.Complete(review, dateTimeService.Now);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCompleted, previousStatus));

            return order;
        }
    }
}
