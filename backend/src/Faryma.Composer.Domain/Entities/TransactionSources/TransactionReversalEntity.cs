namespace Faryma.Composer.Domain.Entities.TransactionSources
{
    /// <summary>
    /// Отмена транзакции
    /// </summary>
    public sealed class TransactionReversalEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Причина отмены
        /// </summary>
        public required string Reason { get; set; }

        public long ReversedTransactionId { get; set; }
        public long ReversalTransactionId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Транзакция, которую отменили
        /// </summary>
        public required TransactionEntity ReversedTransaction { get; set; }

        /// <summary>
        /// Транзакция отмены
        /// </summary>
        public TransactionEntity ReversalTransaction { get; set; } = null!;
    }
}
