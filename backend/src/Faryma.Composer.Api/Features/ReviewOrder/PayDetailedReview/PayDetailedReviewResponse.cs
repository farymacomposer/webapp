using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ReviewOrder.PayDetailedReview
{
    /// <summary>
    /// Ответ на запрос оплаты подробного разбора заказа
    /// </summary>
    public sealed record PayDetailedReviewResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }

        /// <summary>
        /// Id платежа
        /// </summary>
        public long? PaymentTransactionId { get; init; }

        /// <summary>
        /// Id погашения жетона
        /// </summary>
        public long? UserEntitlementRedemptionId { get; init; }
    }
}
