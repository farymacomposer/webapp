using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Отмена транзакции
    /// </summary>
    [Index(nameof(ReversedTransactionId), IsUnique = true)]
    [Index(nameof(ReversalTransactionId), IsUnique = true)]
    [Table("transaction_reversals")]
    public sealed class TransactionReversalEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Причина отмены
        /// </summary>
        [MaxLength(100)]
        public string? Reason { get; set; }

        public long ReversedTransactionId { get; set; }
        public long ReversalTransactionId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Транзакция, которую отменили
        /// </summary>
        [ForeignKey(nameof(ReversedTransactionId))]
        public required TransactionEntity ReversedTransaction { get; set; }

        /// <summary>
        /// Транзакция отмены
        /// </summary>
        [ForeignKey(nameof(ReversalTransactionId))]
        public TransactionEntity ReversalTransaction { get; set; } = null!;
    }
}