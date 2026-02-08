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
        /// Тип транзакции
        /// </summary>
        [Column(TypeName = DbEnumConst.TransactionKindEnum)]
        public required TransactionKind Kind { get; set; }

        /// <summary>
        /// Зачисление
        /// </summary>
        public required long Credit { get; set; }

        /// <summary>
        /// Списание
        /// </summary>
        public required long Debit { get; set; }

        /// <summary>
        /// Сумма операции в зависимости от направления
        /// </summary>
        [NotMapped]
        public long SignedAmount => Credit - Debit;

        public Guid UserNicknameAccountId { get; set; }
        public long TransactionSourceId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Счет псевдонима пользователя
        /// </summary>
        [ForeignKey(nameof(UserNicknameAccountId))]
        public required UserNicknameAccountEntity UserNicknameAccount { get; set; }

        /// <summary>
        /// Источник транзакции
        /// </summary>
        [ForeignKey(nameof(TransactionSourceId))]
        public required TransactionSourceEntity TransactionSource { get; set; }
    }
}