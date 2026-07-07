using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class TrackArtistEntityConfiguration : IEntityTypeConfiguration<TrackArtistEntity>
    {
        public void Configure(EntityTypeBuilder<TrackArtistEntity> builder)
        {
            builder.ToTable("track_artists");

            builder.HasIndex(x => x.NormalizedName)
                .IsUnique();
        }
    }
}
