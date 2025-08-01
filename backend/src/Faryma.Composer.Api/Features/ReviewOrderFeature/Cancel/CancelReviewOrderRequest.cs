using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Cancel
{
    /// <summary>
    /// Запрос отмены заказа на разбор
    /// </summary>
    public sealed record CancelReviewOrderRequest
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        [Required]
        public required long ReviewOrderId { get; set; }

        public CancelCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
            };
        }
    }
}