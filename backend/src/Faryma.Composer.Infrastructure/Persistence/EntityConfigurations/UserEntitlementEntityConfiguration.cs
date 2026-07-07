using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserEntitlementEntityConfiguration : IEntityTypeConfiguration<UserEntitlementEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntitlementEntity> builder)
        {
            builder.ToTable("user_entitlements");

            builder.Property(x => x.Target)
                .HasColumnType(DbContextHelper.UserEntitlementTargetEnum);

            builder.HasOne(x => x.UserNickname)
                .WithMany(x => x.Entitlements)
                .HasForeignKey(x => x.UserNicknameId);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedUserEntitlements)
                .HasForeignKey(x => x.CreatedByUserId);
        }
    }
}
