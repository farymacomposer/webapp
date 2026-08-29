using Faryma.Composer.Application.Features.AppSettings;
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

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateDonation
{
    public sealed class CreateDonationHandler(
        UserStore userStore,
        UserNicknameService userNicknameService,
        ReviewOrderStore reviewOrderStore,
        ReviewOrderService reviewOrderService,
        AppSettingsService appSettingsService,
        TransactionStore transactionStore,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<CreateDonationCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateDonationCommand command, CancellationToken ct)
        {
            UserEntity createdByUser = await userStore.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);
            ComposerStreamEntity nearestStream = await reviewOrderStore.HasOrders(userNickname, ct)
                ? await reviewOrderStore.GetNearestStream(ComposerStreamType.Donation, ct)
                : await reviewOrderStore.GetNearestStream(ct);

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(command.TrackDurationSeconds);

            ReviewOrderEntity order = reviewOrderStore.CreateOrder(
                ReviewOrderType.Donation,
                ReviewOrderStatus.Preorder,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
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

            order.Pay(requiredAmount);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }
    }
}
