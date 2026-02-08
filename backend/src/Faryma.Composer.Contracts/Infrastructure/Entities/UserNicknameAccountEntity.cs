using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Счет псевдонима пользователя
    /// </summary>
    [Table("user_nickname_accounts")]
    public sealed class UserNicknameAccountEntity : PersonalEntity
    {
        [Timestamp]
        public uint Version { get; set; }

        /// <summary>
        /// Текущий баланс
        /// </summary>
        public long Balance { get; set; }

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