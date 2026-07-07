using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Псевдоним пользователя
    /// </summary>
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
        public UserEntity? User { get; set; }

        /// <summary>
        /// Счет пользователя
        /// </summary>
        public UserNicknameAccountEntity Account { get; set; } = null!;

        /// <summary>
        /// Загруженные треки
        /// </summary>
        public ICollection<TrackEntity> UploadedTracks { get; set; } = [];

        /// <summary>
        /// Заказы разборов треков
        /// </summary>
        public ICollection<ReviewOrderEntity> ReviewOrders { get; set; } = [];

        /// <summary>
        /// Жетоны пользователя
        /// </summary>
        public ICollection<UserEntitlementEntity> Entitlements { get; set; } = [];
    }
}
