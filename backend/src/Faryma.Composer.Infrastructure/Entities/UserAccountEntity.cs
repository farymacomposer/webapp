using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Счет пользователя
    /// </summary>
    public sealed class UserAccountEntity : PersonalEntity
    {
        /// <summary>
        /// Текущий баланс
        /// </summary>
        public decimal Balance { get; set; }

        public Guid UserNicknameId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [ForeignKey(nameof(UserNicknameId))]
        public required UserNicknameEntity UserNickname { get; set; }

        /// <summary>
        /// Операции по счету
        /// </summary>
        public ICollection<TransactionEntity> Transactions { get; set; } = [];
    }
}