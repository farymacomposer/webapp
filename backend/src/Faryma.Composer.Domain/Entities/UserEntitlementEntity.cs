using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Пользовательское право на неденежное покрытие стоимости: жетон
    /// </summary>
    public sealed class UserEntitlementEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время выдачи
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время погашения
        /// </summary>
        public DateTime? RedeemedAt { get; set; }

        /// <summary>
        /// Дата и время отмены
        /// </summary>
        public DateTime? CanceledAt { get; set; }

        /// <summary>
        /// Заказ или услуга, которую право может покрывать
        /// </summary>
        public required UserEntitlementTarget Target { get; set; }

        public Guid UserNicknameId { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Псевдоним пользователя, которому выдано право
        /// </summary>
        public required UserNicknameEntity UserNickname { get; set; }

        /// <summary>
        /// Пользователь, выдавший право
        /// </summary>
        public required UserEntity CreatedByUser { get; set; }

        /// <summary>
        /// Погашение права
        /// </summary>
        public UserEntitlementRedemptionEntity? Redemption { get; set; }
    }
}
