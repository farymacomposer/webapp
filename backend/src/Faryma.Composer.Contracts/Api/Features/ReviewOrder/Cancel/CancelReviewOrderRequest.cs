using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Cancel
{
    /// <summary>
    /// Запрос отмены заказа
    /// </summary>
    public sealed record CancelReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Причина отмены заказа
        /// </summary>
        [Required]
        public required string CancelReason { get; init; }
    }
}
