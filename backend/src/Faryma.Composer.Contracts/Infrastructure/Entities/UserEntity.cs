using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    [Table("users")]
    public sealed class UserEntity : IdentityUser<Guid>
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [MaxLength(40)]
        public override required string UserName { get; set; }

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
    }
}