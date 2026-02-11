using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class ReviewEntityConfiguration : IEntityTypeConfiguration<ReviewEntity>
    {
        public void Configure(EntityTypeBuilder<ReviewEntity> builder)
        {
            builder.ToTable("reviews");

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedReviews)
                .HasForeignKey(x => x.CreatedByUserId);

            builder.HasOne(x => x.ReviewOrder)
                .WithOne(x => x.Review)
                .HasForeignKey<ReviewEntity>(x => x.ReviewOrderId);

            builder.HasOne(x => x.Track)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.TrackId);
        }
    }
}
