using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Псевдоним пользователя
    /// </summary>
    [Index(nameof(NormalizedNickname), IsUnique = true)]
    public sealed class UserNicknameEntity : PersonalEntity
    {
        /// <summary>
        /// Псевдоним
        /// </summary>
        public required string Nickname { get; set; }

        public required string NormalizedNickname { get; set; }

        public Guid? UserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь системы
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public UserEntity? User { get; set; }

        /// <summary>
        /// Счет пользователя
        /// </summary>
        public UserAccountEntity Account { get; set; } = null!;

        /// <summary>
        /// Загруженные треки
        /// </summary>
        public ICollection<TrackEntity> UploadedTracks { get; set; } = [];

        /// <summary>
        /// Заказы разборов треков
        /// </summary>
        public ICollection<ReviewOrderEntity> ReviewOrders { get; set; } = [];
    }
}