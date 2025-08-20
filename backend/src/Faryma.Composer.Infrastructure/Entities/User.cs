using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public sealed class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public override required string UserName { get; set; }

        /// <summary>
        /// Дата и время регистрации
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Псевдонимы пользователя
        /// </summary>
        public ICollection<UserNickname> UserNicknames { get; set; } = [];

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public ICollection<TrackArtist> AssociatedArtists { get; set; } = [];

        /// <summary>
        /// Оценки треков
        /// </summary>
        public ICollection<UserTrackRating> TrackRatings { get; set; } = [];
    }
}