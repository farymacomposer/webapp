using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserNicknameAccountEntityConfiguration : IEntityTypeConfiguration<UserNicknameAccountEntity>
    {
        public void Configure(EntityTypeBuilder<UserNicknameAccountEntity> builder)
        {
            builder.ToTable("user_nickname_accounts");

            builder.Property(x => x.Version)
                .IsRowVersion();

            builder.HasOne(x => x.UserNickname)
                .WithOne(x => x.Account)
                .HasForeignKey<UserNicknameAccountEntity>(x => x.UserNicknameId);
        }
    }
}
