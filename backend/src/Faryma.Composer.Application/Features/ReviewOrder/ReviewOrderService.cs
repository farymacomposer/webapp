using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    public sealed class ReviewOrderService(
        UnitOfWork uow,
        UserManager<UserEntity> userManager,
        AppSettingsService appSettingsService)
    {
        public async Task<UserEntity> GetUser(Guid userId, CancellationToken ct)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new ReviewOrderException("Пользователь не найден");
        }

        public async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname, CancellationToken ct)
        {
            if (await UserNicknameHasOrders(userNickname, ct))
            {
                return await uow.ComposerStreamStore.FindNearest(ComposerStreamType.Donation, ct);
            }
            else
            {
                return await uow.ComposerStreamStore.FindNearest(ct);
            }
        }

        public long GetTrackRequiredAmount(int? trackDurationSeconds)
        {
            AppSettingsEntity settings = appSettingsService.Settings;
            long result = settings.ReviewOrderNominalPrice;

            if (trackDurationSeconds > settings.IncludedTrackDurationSeconds)
            {
                int extraTrackSeconds = trackDurationSeconds.Value - settings.IncludedTrackDurationSeconds;
                result += extraTrackSeconds * settings.ReviewOrderExtraTrackSecondPrice;
            }

            return result;
        }

        public void CreateAccountTopUpAndPayment(
            AccountTopUpProvider topUpProvider,
            long paymentAmount,
            UserEntity createdByUser,
            UserNicknameEntity userNickname,
            ReviewOrderEntity order)
        {
            uow.TransactionStore.CreateAccountTopUp(
                topUpProvider,
                paymentAmount,
                userNickname.Account,
                createdByUser);

            uow.TransactionStore.CreatePayment(
                paymentAmount,
                userNickname.Account,
                order);
        }

        public void CreateAndRedeemAdminCoverage(
            ReviewOrderEntity order,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            UserEntitlementTarget target = order.Type switch
            {
                ReviewOrderType.OutOfQueue => UserEntitlementTarget.OutOfQueueReviewOrder,
                ReviewOrderType.Free => UserEntitlementTarget.FreeReviewOrder,
                _ => throw new ReviewOrderException("Тип заказа не поддерживает жетон", order)
            };

            UserEntitlementEntity entitlement = uow.UserEntitlementStore.Create(
                target,
                userNickname,
                createdByUser);

            uow.UserEntitlementStore.Redeem(
                entitlement,
                target,
                createdByUser,
                order);
        }

        /// <summary>
        /// Возвращает заказ в статусе InProgress, если он существует
        /// </summary>
        public Task<ReviewOrderEntity?> FindInProgress(CancellationToken ct = default)
        {
            return uow.Context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress, ct);
        }

        private Task<bool> UserNicknameHasOrders(UserNicknameEntity userNickname, CancellationToken ct) =>
            uow.Context.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0, ct);
    }
}
