using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.Pay
{
    /// <summary>
    /// Ответ на запрос оплаты заказа разбора трека
    /// </summary>
    public sealed record PayReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }

        /// <summary>
        /// Id платежа
        /// </summary>
        public required long PaymentTransactionId { get; init; }
    }
}
