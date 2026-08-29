using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.TakeInProgress
{
    public sealed class TakeInProgressHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        OrderQueueService orderQueueService,
        OrderQueueEventChannel orderQueueEventChannel,
        DateTimeService dateTimeService) : IRequestHandler<TakeInProgressCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(TakeInProgressCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await uow.ReviewOrderStore.Get(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            ComposerStreamEntity liveStream = await uow.ComposerStreamStore.FindLive(ct)
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            long? idOrderInProgress = await reviewOrderService.FindInProgress(ct);
            if (idOrderInProgress is not null && idOrderInProgress != command.ReviewOrderId)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ, пока заказ Id: {idOrderInProgress} находится в работе", order);
            }

            OrderQueuePosition position = await orderQueueService.GetCurrentQueuePosition(order);

            order.TakeInProgress(
                liveStream,
                position.Category.QueueCategory,
                dateTimeService.Now);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderTaken, previousStatus);

            return order;
        }
    }
}
