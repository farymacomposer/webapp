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

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateOutOfQueue
{
    public sealed class CreateOutOfQueueHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateOutOfQueueCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateOutOfQueueCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity? nearestStream = await uow.ComposerStreamStore.FindNearest(ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            ReviewOrderStatus status = command.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                ReviewOrderType.OutOfQueue,
                status,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
                command.UserComment,
                nearestStream,
                userNickname,
                createdByUser);

            reviewOrderService.CreateAndRedeemAdminCoverage(
                order,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }
    }
}
