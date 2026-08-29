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
        AppDbContext context,
        DateTimeService dateTimeService,
        UserStore userStore,
        ReviewStore reviewStore,
        ReviewOrderStore reviewOrderStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CompleteCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CompleteCommand command, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            UserEntity createdByUser = await userStore.GetUser(command.CreatedByUserId, ct);
            ReviewEntity review = reviewStore.Create(order, command.Rating, createdByUser);

            order.Complete(review, dateTimeService.Now);

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCompleted, previousStatus);

            return order;
        }
    }
}
