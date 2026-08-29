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

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateOutOfQueue
{
    public sealed class CreateOutOfQueueHandler(
        UserStore userStore,
        ReviewOrderStore reviewOrderStore,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        UserEntitlementStore userEntitlementStore,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<CreateOutOfQueueCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateOutOfQueueCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await userStore.GetUser(ct);
            ComposerStreamEntity nearestStream = await reviewOrderStore.GetNearestStream(ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ReviewOrderStatus status = command.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;

            ReviewOrderEntity order = reviewOrderStore.CreateOrder(
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

            userEntitlementStore.CreateAndRedeemAdminCoverage(
                order,
                userNickname,
                createdByUser);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }
    }
}
