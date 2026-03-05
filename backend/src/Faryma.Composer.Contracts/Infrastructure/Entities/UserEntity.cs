using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public sealed class UserEntity : IdentityUser<Guid>
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public override required string UserName { get; set; }

        /// <summary>
        /// Идентификатор пользователя в Twitch
        /// </summary>
        public string? TwitchUserId { get; set; }

        /// <summary>
        /// Логин пользователя в Twitch
        /// </summary>
        public string? TwitchLogin { get; set; }

        /// <summary>
        /// Дата и время регистрации
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Псевдонимы пользователя
        /// </summary>
        public ICollection<UserNicknameEntity> UserNicknames { get; set; } = [];

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public ICollection<TrackArtistEntity> AssociatedArtists { get; set; } = [];

        /// <summary>
        /// Оценки треков
        /// </summary>
        public ICollection<UserTrackRatingEntity> TrackRatings { get; set; } = [];

        /// <summary>
        /// Стримы, созданные композитором
        /// </summary>
        public ICollection<ComposerStreamEntity> CreatedComposerStreams { get; set; } = [];

        /// <summary>
        /// Разборы треков, созданные композитором
        /// </summary>
        public ICollection<ReviewEntity> CreatedReviews { get; set; } = [];

        /// <summary>
        /// Треки, созданные пользователем
        /// </summary>
        public ICollection<TrackEntity> CreatedTracks { get; set; } = [];

        /// <summary>
        /// Источники транзакций, созданные пользователем
        /// </summary>
        public ICollection<TransactionSourceEntity> CreatedTransactionSources { get; set; } = [];

        /// <summary>
        /// Refresh токены пользователя
        /// </summary>
        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
    }
}