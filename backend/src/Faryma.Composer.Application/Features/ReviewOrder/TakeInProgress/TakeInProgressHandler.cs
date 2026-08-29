using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.TakeInProgress
{
    public sealed class TakeInProgressHandler(
        ReviewOrderStore reviewOrderStore,
        OrderQueueService orderQueueService,
        DateTimeContext dateTimeContext,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<TakeInProgressCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(TakeInProgressCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            ComposerStreamEntity liveStream = await reviewOrderStore.GetLiveStream(ct);

            ReviewOrderEntity? orderInProgress = await reviewOrderStore.FindOrderInProgress(ct);
            if (orderInProgress is not null && orderInProgress.Id != command.ReviewOrderId)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ, пока заказ Id: {orderInProgress.Id} находится в работе", order);
            }

            OrderQueuePosition position = await orderQueueService.GetCurrentQueuePosition(order);

            order.TakeInProgress(
                liveStream,
                position.Category.QueueCategory,
                dateTimeContext.Now);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderTaken, previousStatus);

            return order;
        }
    }
}
