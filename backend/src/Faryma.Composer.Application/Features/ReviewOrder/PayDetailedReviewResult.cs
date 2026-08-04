using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    /// <summary>
    /// Результат оплаты подробного разбора деньгами или жетоном.
    /// </summary>
    public sealed record PayDetailedReviewResult
    {
        /// <summary>
        /// Заказ с актуальным состоянием подробного разбора.
        /// </summary>
        public required ReviewOrderEntity ReviewOrder { get; init; }

        /// <summary>
        /// Денежная транзакция оплаты, если подробный разбор оплачен деньгами.
        /// </summary>
        public TransactionEntity? PaymentTransaction { get; init; }

        /// <summary>
        /// Погашение жетона, если подробный разбор оплачен жетоном.
        /// </summary>
        public UserEntitlementRedemptionEntity? UserEntitlementRedemption { get; init; }
    }
}
