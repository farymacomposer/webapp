using Faryma.Composer.Domain.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations.TransactionSources
{
    internal sealed class TransactionReversalEntityConfiguration : IEntityTypeConfiguration<TransactionReversalEntity>
    {
        public void Configure(EntityTypeBuilder<TransactionReversalEntity> builder)
        {
            builder.ToTable("transaction_reversals");

            builder.HasIndex(x => x.ReversedTransactionId)
                .IsUnique();

            builder.HasIndex(x => x.ReversalTransactionId)
                .IsUnique();

            builder.Property(x => x.Reason)
                .HasMaxLength(100);

            builder.HasOne(x => x.ReversedTransaction)
                .WithMany()
                .HasForeignKey(x => x.ReversedTransactionId);

            builder.HasOne(x => x.ReversalTransaction)
                .WithMany()
                .HasForeignKey(x => x.ReversalTransactionId);
        }
    }
}
