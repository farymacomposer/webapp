using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze
{
    /// <summary>
    /// Запрос заморозки заказа на разбор
    /// </summary>
    public sealed record FreezeReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
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