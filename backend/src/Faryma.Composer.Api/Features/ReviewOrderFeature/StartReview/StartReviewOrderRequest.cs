using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.StartReview
{
    /// <summary>
    /// Запрос начала разбора трека
    /// </summary>
    public sealed record StartReviewOrderRequest
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        public StartReviewCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
            };
        }
    }
}