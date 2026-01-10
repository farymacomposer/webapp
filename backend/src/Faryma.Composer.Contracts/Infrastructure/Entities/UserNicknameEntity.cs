using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Псевдоним пользователя
    /// </summary>
    [Index(nameof(NormalizedNickname), IsUnique = true)]
    [Table("user_nicknames")]
    public sealed class UserNicknameEntity : PersonalEntity
    {
        /// <summary>
        /// Псевдоним
        /// </summary>
        [MaxLength(40)]
        public required string Nickname { get; set; }

        [MaxLength(40)]
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