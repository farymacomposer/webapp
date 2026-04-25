using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder
                .Property(x => x.TwitchUserId)
                .HasMaxLength(50);

            builder
                .Property(x => x.TwitchLogin)
                .HasMaxLength(100);

            builder
                .HasIndex(x => x.TwitchUserId)
                .IsUnique()
                .HasFilter("\"TwitchUserId\" IS NOT NULL");
        }
    }
}
