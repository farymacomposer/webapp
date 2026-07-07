using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Счет псевдонима пользователя
    /// </summary>
    public sealed class UserNicknameAccountEntity : PersonalEntity
    {
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
        public required UserNicknameEntity UserNickname { get; set; }

        /// <summary>
        /// Операции по счету
        /// </summary>
        public ICollection<TransactionEntity> Transactions { get; set; } = [];
    }
}
