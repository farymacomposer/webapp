using Faryma.Composer.Domain.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations.TransactionSources
{
    internal sealed class AccountTopUpEntityConfiguration : IEntityTypeConfiguration<AccountTopUpEntity>
    {
        public void Configure(EntityTypeBuilder<AccountTopUpEntity> builder)
        {
            builder.ToTable("account_top_ups");

            builder.Property(x => x.Provider)
                .HasColumnType(DbContextHelper.AccountTopUpProviderEnum);

            builder.HasOne(x => x.UserNicknameAccount)
                .WithMany()
                .HasForeignKey(x => x.UserNicknameAccountId);
        }
    }
}
