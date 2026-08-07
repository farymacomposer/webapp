using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
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
            ReviewOrderEntity order = await uow.ReviewOrderStore.Get(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            reviewOrderService.CreateAccountTopUpAndPayment(
                command.TopUpProvider,
                command.PaymentAmount,
                createdByUser,
                userNickname,
                order);

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(order.TrackDurationSeconds);

            order.Pay(requiredAmount);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderMovedUp, previousStatus);

            return order;
        }
    }
}
