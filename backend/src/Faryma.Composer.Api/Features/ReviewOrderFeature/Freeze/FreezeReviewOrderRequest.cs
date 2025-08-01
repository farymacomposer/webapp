using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze
{
    /// <summary>
    /// Запрос заморозки заказа на разбор
    /// </summary>
    public sealed record FreezeReviewOrderRequest
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        [Required]
        public required long ReviewOrderId { get; set; }

        public FreezeCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
            };
        }
    }
}