using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations.TransactionSources
{
    internal sealed class TransactionSourceEntityConfiguration : IEntityTypeConfiguration<TransactionSourceEntity>
    {
        public void Configure(EntityTypeBuilder<TransactionSourceEntity> builder)
        {
            builder.ToTable("transaction_sources");

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedTransactionSources)
                .HasForeignKey(x => x.CreatedByUserId);
        }
    }
}