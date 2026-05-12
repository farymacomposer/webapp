using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class IdempotencyRecordEntityConfiguration : IEntityTypeConfiguration<IdempotencyRecordEntity>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecordEntity> builder)
        {
            builder.ToTable("idempotency_records");

            builder
                .Property(x => x.EndpointKey)
                .HasMaxLength(300)
                .IsRequired();

            builder
                .Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            builder
                .Property(x => x.ResponseJson)
                .HasColumnType("jsonb");

            builder
                .HasIndex(x => new { x.EndpointKey, x.UserId, x.IdempotencyKey })
                .IsUnique();

            builder.HasIndex(x => x.ExpiresAt);
        }
    }
}
