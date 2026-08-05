using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Pay
{
    public sealed class PayHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<PayCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(PayCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderService.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно оплатить заказ", order);
            }

            if (order.Type is not (ReviewOrderType.Donation or ReviewOrderType.Free))
            {
                throw new ReviewOrderException("Тип заказа не поддерживает денежную оплату", order);
            }

            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            reviewOrderService.CreateAccountTopUpAndPayment(
                command.TopUpProvider,
                command.PaymentAmount,
                createdByUser,
                userNickname,
                order);

            RecalculateCheckoutStatus(order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderMovedUp, previousStatus));

            return order;
        }
    }
}
