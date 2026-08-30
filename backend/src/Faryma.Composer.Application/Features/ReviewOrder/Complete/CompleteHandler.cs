using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Infrastructure.Features.User;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Complete
{
    public sealed class CompleteHandler(
        ReviewOrderStore reviewOrderStore,
        UserStore userStore,
        ReviewStore reviewStore,
        DateTimeContext dateTimeContext,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<CompleteCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CompleteCommand command, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            order.ThrowIfCannotBeComplete();

            UserEntity createdByUser = await userStore.GetUser(ct);
            ReviewEntity review = reviewStore.CreateReview(order, command.Rating, createdByUser);

            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            order.Complete(review, dateTimeContext.Now);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCompleted, previousStatus);

            return order;
        }
    }
}
