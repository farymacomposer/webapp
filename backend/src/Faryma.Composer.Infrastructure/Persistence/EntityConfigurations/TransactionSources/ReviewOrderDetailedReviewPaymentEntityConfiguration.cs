using Faryma.Composer.Domain.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations.TransactionSources
{
    internal sealed class ReviewOrderDetailedReviewPaymentEntityConfiguration : IEntityTypeConfiguration<ReviewOrderDetailedReviewPaymentEntity>
    {
        public void Configure(EntityTypeBuilder<ReviewOrderDetailedReviewPaymentEntity> builder)
        {
            builder.ToTable("review_order_detailed_review_payments");

            builder.HasIndex(x => x.ReviewOrderId)
                .IsUnique();

            builder.HasOne(x => x.ReviewOrder)
                .WithOne(x => x.DetailedReviewPayment)
                .HasForeignKey<ReviewOrderDetailedReviewPaymentEntity>(x => x.ReviewOrderId);
        }
    }
}
