using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateFree
{
    public sealed class CreateFreeHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateFreeCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateFreeCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity nearestStream = await reviewOrderService.FindNearestStream(userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            ReviewOrderStatus status = command.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                ReviewOrderType.Free,
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

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }
    }
}
