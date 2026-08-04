using Faryma.Composer.Application.Features.AppSettings;
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

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateDonation
{
    public sealed class CreateDonationHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateDonationCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateDonationCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity nearestStream = await reviewOrderService.FindNearestStream(userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            long requiredAmount = reviewOrderService.GetTrackRequiredAmount(command.TrackDurationSeconds);

            (ReviewOrderStatus status, long payableAmount) = command.TrackUrl is null
                ? (ReviewOrderStatus.Preorder, requiredAmount)
                : requiredAmount > command.PaymentAmount
                    ? (ReviewOrderStatus.AwaitingPayment, requiredAmount - command.PaymentAmount)
                    : (ReviewOrderStatus.Pending, 0);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
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

            reviewOrderService.CreateAccountTopUpAndPayment(
                command.TopUpProvider,
                command.PaymentAmount,
                createdByUser,
                userNickname,
                order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }
    }
}
