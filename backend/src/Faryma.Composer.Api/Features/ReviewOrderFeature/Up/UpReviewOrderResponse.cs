using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Shared.Dto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Up
{
    /// <summary>
    /// Ответ на запрос поднятия заказа в очереди
    /// </summary>
    public sealed record UpReviewOrderResponse
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