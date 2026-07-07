using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities.TransactionSources
{
    /// <summary>
    /// Базовый источник транзакции (TPT стратегия)
    /// </summary>
    public abstract class TransactionSourceEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время создания
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, создавший источник транзакции
        /// </summary>
        public required UserEntity CreatedByUser { get; set; }

        /// <summary>
        /// Транзакции, относящиеся к этому источнику
        /// </summary>
        public ICollection<TransactionEntity> Transactions { get; set; } = [];
    }
}
