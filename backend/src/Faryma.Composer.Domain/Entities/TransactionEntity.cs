using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Операция по счету
    /// </summary>
    public sealed class TransactionEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время создания
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Тип транзакции
        /// </summary>
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
        public long SignedAmount => Credit - Debit;

        public Guid UserNicknameAccountId { get; set; }
        public long TransactionSourceId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Счет псевдонима пользователя
        /// </summary>
        public required UserNicknameAccountEntity UserNicknameAccount { get; set; }

        /// <summary>
        /// Источник транзакции
        /// </summary>
        public required TransactionSourceEntity TransactionSource { get; set; }
    }
}
