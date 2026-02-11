using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class ComposerStreamEntityConfiguration : IEntityTypeConfiguration<ComposerStreamEntity>
    {
        public void Configure(EntityTypeBuilder<ComposerStreamEntity> builder)
        {
            builder.ToTable("composer_streams");

            builder.HasIndex(x => x.EventDate)
                .IsUnique();

            builder.Property(x => x.Type)
                .HasColumnType(DbEnumConst.ComposerStreamTypeEnum);

            builder.Property(x => x.Status)
                .HasColumnType(DbEnumConst.ComposerStreamStatusEnum);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedComposerStreams)
                .HasForeignKey(x => x.CreatedByUserId);
        }
    }
}