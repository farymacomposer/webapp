using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Refresh token для продления пользовательской сессии
    /// </summary>
    public sealed class RefreshTokenEntity : PersonalEntity
    {
        public uint Version { get; set; }

        /// <summary>
        /// SHA-256 хэш сырого refresh token
        /// </summary>
        public required string TokenHash { get; set; }

        /// <summary>
        /// Идентификатор семейства пользователей
        /// </summary>
        public required Guid FamilyId { get; set; }

        /// <summary>
        /// Дата создания токена
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата истечения срока действия токена
        /// </summary>
        public required DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Дата отзыва токена
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Хэш замененного токена
        /// </summary>
        public string? ReplacedByTokenHash { get; set; }

        public Guid UserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь
        /// </summary>
        public required UserEntity User { get; set; }

        public bool IsExpired(DateTime now) => ExpiresAt <= now;
    }
}
