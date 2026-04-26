using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserNicknameEntityConfiguration : IEntityTypeConfiguration<UserNicknameEntity>
    {
        public void Configure(EntityTypeBuilder<UserNicknameEntity> builder)
        {
            builder.ToTable("user_nicknames");

            builder.HasIndex(x => x.NormalizedNickname)
                .IsUnique();

            builder.Property(x => x.Nickname)
                .HasMaxLength(40);

            builder.Property(x => x.NormalizedNickname)
                .HasMaxLength(40);

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserNicknames)
                .HasForeignKey(x => x.UserId);
        }
    }
}
