using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class UserEntitlementStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public Task<UserEntitlementEntity?> FindById(long id, CancellationToken ct = default)
        {
            IQueryable<UserEntitlementEntity> query = context.UserEntitlements
                .Include(x => x.UserNickname)
                .Include(x => x.Redemption)
                .Where(x => x.Id == id);

            return query.FirstOrDefaultAsync(ct);
        }

        public UserEntitlementEntity Create(
            UserEntitlementTarget target,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            ValidateTarget(target);

            return context.Add(new UserEntitlementEntity
            {
                CreatedAt = dateTimeService.Now,
                Target = target,
                UserNickname = userNickname,
                CreatedByUser = createdByUser,
            }).Entity;
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

            return context.Add(new UserEntitlementRedemptionEntity
            {
                CreatedAt = dateTimeService.Now,
                Target = target,
                UserEntitlement = entitlement,
                RedeemedByUser = redeemedByUser,
                ReviewOrder = reviewOrder,
                DetailedReview = detailedReview,
            }).Entity;
        }

        public void Cancel(UserEntitlementEntity entitlement)
        {
            if (entitlement.RedeemedAt is not null || entitlement.Redemption is not null)
            {
                throw new InvalidOperationException("Нельзя отменить погашенное право");
            }

            entitlement.CanceledAt = dateTimeService.Now;
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
