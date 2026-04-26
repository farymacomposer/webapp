using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class AppSettingsEntityConfiguration : IEntityTypeConfiguration<AppSettingsEntity>
    {
        public void Configure(EntityTypeBuilder<AppSettingsEntity> builder) => builder.ToTable("app_settings");
    }
}
