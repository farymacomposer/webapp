namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Refresh token для продления пользовательской сессии.
    /// </summary>
    public sealed class RefreshTokenEntity
    {
        public required Guid Id { get; set; }

        public required Guid UserId { get; set; }

        public required UserEntity User { get; set; }

        /// <summary>
        /// SHA-256 хэш сырого refresh token.
        /// </summary>
        public required string TokenHash { get; set; }

        public required Guid FamilyId { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }
    }
}