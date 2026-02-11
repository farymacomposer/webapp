using Faryma.Composer.Contracts.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
    {
        public void Configure(EntityTypeBuilder<TransactionEntity> builder)
        {
            builder.ToTable("transactions");

            builder.Property(x => x.Kind)
                .HasColumnType(DbEnumConst.TransactionKindEnum);

            builder.Ignore(x => x.SignedAmount);

            builder.HasOne(x => x.UserNicknameAccount)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserNicknameAccountId);

            builder.HasOne(x => x.TransactionSource)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.TransactionSourceId);
        }
    }
}