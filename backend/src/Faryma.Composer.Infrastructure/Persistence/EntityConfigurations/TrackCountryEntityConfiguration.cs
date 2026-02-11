using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class TrackCountryEntityConfiguration : IEntityTypeConfiguration<TrackCountryEntity>
    {
        public void Configure(EntityTypeBuilder<TrackCountryEntity> builder) => builder.ToTable("track_countries");
    }
}