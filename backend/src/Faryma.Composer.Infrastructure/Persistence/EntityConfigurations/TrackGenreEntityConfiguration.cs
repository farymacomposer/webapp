using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class TrackGenreEntityConfiguration : IEntityTypeConfiguration<TrackGenreEntity>
    {
        public void Configure(EntityTypeBuilder<TrackGenreEntity> builder) => builder.ToTable("track_genres");
    }
}