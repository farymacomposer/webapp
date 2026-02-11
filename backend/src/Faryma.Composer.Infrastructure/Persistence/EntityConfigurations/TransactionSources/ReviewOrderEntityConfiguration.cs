using Faryma.Composer.Contracts.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations.TransactionSources
{
    internal sealed class ReviewOrderEntityConfiguration : IEntityTypeConfiguration<ReviewOrderEntity>
    {
        public void Configure(EntityTypeBuilder<ReviewOrderEntity> builder)
        {
            builder.ToTable("review_orders");

            builder.Property(x => x.MainNickname)
                .HasMaxLength(40);

            builder.Property(x => x.MainNormalizedNickname)
                .HasMaxLength(40);

            builder.Property(x => x.UserComment)
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .HasColumnType(DbEnumConst.ReviewOrderTypeEnum);

            builder.Property(x => x.Status)
                .HasColumnType(DbEnumConst.ReviewOrderStatusEnum);

            builder.Property(x => x.CategoryType)
                .HasColumnType(DbEnumConst.OrderCategoryTypeEnum);

            builder.HasOne(x => x.Track)
                .WithMany(x => x.ReviewOrders)
                .HasForeignKey(x => x.TrackId);

            builder.HasOne(x => x.CreationStream)
                .WithMany(x => x.CreatedReviewOrders)
                .HasForeignKey(x => x.CreationStreamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcessingStream)
                .WithMany(x => x.ProcessedReviewOrders)
                .HasForeignKey(x => x.ProcessingStreamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}