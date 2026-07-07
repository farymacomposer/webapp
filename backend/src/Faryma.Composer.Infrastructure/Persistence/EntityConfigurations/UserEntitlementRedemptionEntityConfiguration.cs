using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserEntitlementRedemptionEntityConfiguration : IEntityTypeConfiguration<UserEntitlementRedemptionEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntitlementRedemptionEntity> builder)
        {
            builder.ToTable("user_entitlement_redemptions");

            builder.HasIndex(x => x.UserEntitlementId)
                .IsUnique();

            builder.HasIndex(x => x.ReviewOrderId)
                .IsUnique();

            builder.HasIndex(x => x.ReviewOrderDetailedReviewPaymentId)
                .IsUnique();

            builder.Property(x => x.Target)
                .HasColumnType(DbContextHelper.UserEntitlementTargetEnum);

            builder.HasOne(x => x.UserEntitlement)
                .WithOne(x => x.Redemption)
                .HasForeignKey<UserEntitlementRedemptionEntity>(x => x.UserEntitlementId);

            builder.HasOne(x => x.RedeemedByUser)
                .WithMany(x => x.RedeemedUserEntitlements)
                .HasForeignKey(x => x.RedeemedByUserId);

            builder.HasOne(x => x.ReviewOrder)
                .WithOne(x => x.CoverageRedemption)
                .HasForeignKey<UserEntitlementRedemptionEntity>(x => x.ReviewOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DetailedReview)
                .WithOne(x => x.CoverageRedemption)
                .HasForeignKey<UserEntitlementRedemptionEntity>(x => x.ReviewOrderDetailedReviewPaymentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
