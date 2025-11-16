using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Операция по счету
    /// </summary>
    public sealed class TransactionEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время совершения операции
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Тип операции
        /// </summary>
        [Column(TypeName = DbContextHelper.TransactionTypeEnum)]
        public required TransactionType Type { get; set; }

        /// <summary>
        /// Сумма операции
        /// </summary>
        public required decimal Amount { get; set; }

        public Guid UserAccountId { get; set; }
        public long? ReviewOrderId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Счет пользователя
        /// </summary>
        [ForeignKey(nameof(UserAccountId))]
        public required UserAccountEntity Account { get; set; }

        /// <summary>
        /// Заказ разбора треков
        /// </summary>
        [ForeignKey(nameof(ReviewOrderId))]
        public ReviewOrderEntity? ReviewOrder { get; set; }
    }
}