using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Счет пользователя
    /// </summary>
    public sealed class UserAccount : PersonalEntity
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
        public required UserNickname UserNickname { get; set; }

        /// <summary>
        /// Операции по счету
        /// </summary>
        public ICollection<Transaction> Transactions { get; set; } = [];
    }
}