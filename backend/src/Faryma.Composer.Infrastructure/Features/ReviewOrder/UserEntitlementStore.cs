using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.ReviewOrder
{
    public sealed class UserEntitlementStore(AppDbContext appDbContext, DateTimeService dateTimeService)
    {
        public void CreateAndRedeemAdminCoverage(
            ReviewOrderEntity order,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            UserEntitlementTarget target = order.Type switch
            {
                ReviewOrderType.OutOfQueue => UserEntitlementTarget.OutOfQueueReviewOrder,
                ReviewOrderType.Free => UserEntitlementTarget.FreeReviewOrder,
                _ => throw new NotSupportedException($"Тип заказа {order.Type} не поддерживает жетон")
            };

            UserEntitlementEntity entitlement = Create(
                target,
                userNickname,
                createdByUser);

            Redeem(
                entitlement,
                target,
                createdByUser,
                order);
        }

        public UserEntitlementEntity Create(
            UserEntitlementTarget target,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            ValidateTarget(target);

            return appDbContext.Add(new UserEntitlementEntity
            {
                CreatedAt = dateTimeService.Now,
                Target = target,
                UserNickname = userNickname,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public Task<UserEntitlementEntity?> FindById(long id, CancellationToken ct)
        {
            IQueryable<UserEntitlementEntity> query = appDbContext.UserEntitlements
                .Include(x => x.UserNickname)
                .Include(x => x.Redemption)
                .Where(x => x.Id == id);

            return query.FirstOrDefaultAsync(ct);
        }

        public UserEntitlementRedemptionEntity Redeem(
            UserEntitlementEntity entitlement,
            UserEntitlementTarget target,
            UserEntity redeemedByUser,
            ReviewOrderEntity? reviewOrder = null,
            ReviewOrderDetailedReviewPaymentEntity? detailedReview = null)
        {
            ValidateTarget(target);

            if (entitlement.Target != target)
            {
                throw new InvalidOperationException("Право не применимо к выбранной услуге");
            }

            if (entitlement.RedeemedAt is not null || entitlement.Redemption is not null)
            {
                throw new InvalidOperationException("Право уже погашено");
            }

            if (entitlement.CanceledAt is not null)
            {
                throw new InvalidOperationException("Право отменено");
            }

            entitlement.RedeemedAt = dateTimeService.Now;

            return appDbContext.Add(new UserEntitlementRedemptionEntity
            {
                CreatedAt = dateTimeService.Now,
                Target = target,
                UserEntitlement = entitlement,
                RedeemedByUser = redeemedByUser,
                ReviewOrder = reviewOrder,
                DetailedReview = detailedReview,
            }).Entity;
        }

        private static void ValidateTarget(UserEntitlementTarget target)
        {
            if (!Enum.IsDefined(target) || target == UserEntitlementTarget.Unspecified)
            {
                throw new ArgumentException("Цель права должна быть указана", nameof(target));
            }
        }
    }
}
