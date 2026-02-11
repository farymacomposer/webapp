using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class TrackEntityConfiguration : IEntityTypeConfiguration<TrackEntity>
    {
        public void Configure(EntityTypeBuilder<TrackEntity> builder)
        {
            builder.ToTable("tracks");

            builder.OwnsMany(x => x.Tags, x => x.ToJson());

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedTracks)
                .HasForeignKey(x => x.CreatedByUserId);

            builder.HasOne(x => x.AddedBy)
                .WithMany(x => x.UploadedTracks)
                .HasForeignKey(x => x.AddedByUserNicknameId);

            builder.HasOne(x => x.Country)
                .WithMany(x => x.Tracks)
                .HasForeignKey(x => x.CountryId);
        }
    }
}