using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.Cancel
{
    /// <summary>
    /// Запрос отмены заказа
    /// </summary>
    public sealed record CancelReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id заказа должен быть больше нуля")]
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Причина отмены заказа
        /// </summary>
        [Required]
        public required string CancelReason { get; init; }
    }
}
