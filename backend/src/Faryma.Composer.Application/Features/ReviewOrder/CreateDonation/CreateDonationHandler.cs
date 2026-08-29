using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Infrastructure.Features.User;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateDonation
{
    public sealed class CreateDonationHandler(
        AppDbContext context,
        UserStore userStore,
        ReviewOrderStore reviewOrderStore,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        TransactionStore transactionStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateDonationCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateDonationCommand command, CancellationToken ct)
        {
            UserEntity createdByUser = await userStore.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity nearestStream = await reviewOrderStore.FindNearestStream(userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(command.TrackDurationSeconds);

            (ReviewOrderStatus status, long payableAmount) = command.TrackUrl is null
                ? (ReviewOrderStatus.Preorder, requiredAmount)
                : requiredAmount > command.PaymentAmount
                    ? (ReviewOrderStatus.AwaitingPayment, requiredAmount - command.PaymentAmount)
                    : (ReviewOrderStatus.Pending, 0);

            ReviewOrderEntity order = reviewOrderStore.CreateOrder(
                ReviewOrderType.Donation,
                status,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount,
                command.UserComment,
                nearestStream,
                userNickname,
                createdByUser);

            transactionStore.CreateAccountTopUpAndPayment(
                command.TopUpProvider,
                command.PaymentAmount,
                createdByUser,
                userNickname,
                order);

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }
    }
}
