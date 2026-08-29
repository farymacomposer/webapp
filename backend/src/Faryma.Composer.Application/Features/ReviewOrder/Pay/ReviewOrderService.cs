using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Infrastructure.Features.User;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Pay
{
    public sealed class PayHandler(
        ReviewOrderStore reviewOrderStore,
        UserStore userStore,
        UserNicknameService userNicknameService,
        TransactionStore transactionStore,
        ReviewOrderService reviewOrderService,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<PayCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(PayCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await reviewOrderStore.GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            UserEntity createdByUser = await userStore.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            transactionStore.CreateAccountTopUpAndPayment(
                command.TopUpProvider,
                command.PaymentAmount,
                createdByUser,
                userNickname,
                order);

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(order.TrackDurationSeconds);

            order.Pay(requiredAmount);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderMovedUp, previousStatus);

            return order;
        }
    }
}
