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
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateCharity
{
    public sealed class CreateCharityHandler(
        AppDbContext context,
        ReviewOrderStore reviewOrderStore,
        ReviewOrderService reviewOrderService,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateCharityCommand, ReviewOrderEntity>
    {
        public async ValueTask<ReviewOrderEntity> Handle(CreateCharityCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await reviewOrderService.GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity? liveStream = await reviewOrderStore.FindLiveStream(ct);
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Нет запущенного благотворительного стрима");
            }

            ReviewOrderStatus status = command.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;

            ReviewOrderEntity order = reviewOrderStore.Create(
                ReviewOrderType.Charity,
                status,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
                command.UserComment,
                liveStream,
                userNickname,
                createdByUser);

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }
    }
}
