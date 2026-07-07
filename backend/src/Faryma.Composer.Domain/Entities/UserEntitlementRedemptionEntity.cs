using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Погашение пользовательского права на заказ или услугу
    /// </summary>
    public sealed class UserEntitlementRedemptionEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время погашения
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Заказ или услуга, покрытая при погашении
        /// </summary>
        public required UserEntitlementTarget Target { get; set; }

        public long UserEntitlementId { get; set; }
        public Guid RedeemedByUserId { get; set; }
        public long? ReviewOrderId { get; set; }
        public long? ReviewOrderDetailedReviewPaymentId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Погашенное пользовательское право
        /// </summary>
        public required UserEntitlementEntity UserEntitlement { get; set; }

        /// <summary>
        /// Пользователь, выполнивший погашение
        /// </summary>
        public required UserEntity RedeemedByUser { get; set; }

        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity? ReviewOrder { get; set; }

        /// <summary>
        /// Услуга подробного разбора трека
        /// </summary>
        public ReviewOrderDetailedReviewPaymentEntity? DetailedReview { get; set; }
    }
}
