using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Пополнение счета пользователя
    /// </summary>
    public sealed class AccountTopUpEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        public required AccountTopUpProvider Provider { get; set; }

        public Guid UserNicknameAccountId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Счет псевдонима пользователя
        /// </summary>
        public required UserNicknameAccountEntity UserNicknameAccount { get; set; }
    }
}