using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Операция по счету
    /// </summary>
    [Table("transactions")]
    public sealed class TransactionEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время создания
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Направление транзакции (увеличение или уменьшение баланса счета)
        /// </summary>
        [Column(TypeName = DbEnumConst.TransactionDirectionEnum)]
        public required TransactionDirection Direction { get; set; }

        /// <summary>
        /// Тип транзакции
        /// </summary>
        [Column(TypeName = DbEnumConst.TransactionKindEnum)]
        public required TransactionKind Kind { get; set; }

        /// <summary>
        /// Сумма операции
        /// </summary>
        public required decimal Amount { get; set; }

        /// <summary>
        /// Сумма операции в зависимости от направления
        /// </summary>
        [NotMapped]
        public decimal SignedAmount => Direction == TransactionDirection.Debit ? -Amount : Amount;

        public Guid UserAccountId { get; set; }
        public long SourceId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Счет пользователя
        /// </summary>
        [ForeignKey(nameof(UserAccountId))]
        public required UserAccountEntity Account { get; set; }

        /// <summary>
        /// Источник транзакции
        /// </summary>
        [ForeignKey(nameof(SourceId))]
        public required TransactionSourceEntity Source { get; set; }
    }
}