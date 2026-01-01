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
        public string? Reason { get; set; }

        public Guid ReversedByUserId { get; set; }
        public long ReversedTransactionId { get; set; }
        public long? ReversalTransactionId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, который сделал отмену
        /// </summary>
        [ForeignKey(nameof(ReversedByUserId))]
        public required UserEntity ReversedByUser { get; set; }

        /// <summary>
        /// Транзакция, которую отменили
        /// </summary>
        [ForeignKey(nameof(ReversedTransactionId))]
        public required TransactionEntity ReversedTransaction { get; set; }

        /// <summary>
        /// Транзакция отмены
        /// </summary>
        [ForeignKey(nameof(ReversalTransactionId))]
        public required TransactionEntity ReversalTransaction { get; set; }
    }
}