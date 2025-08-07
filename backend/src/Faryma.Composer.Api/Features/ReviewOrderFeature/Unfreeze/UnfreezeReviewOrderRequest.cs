using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Unfreeze
{
    /// <summary>
    /// Запрос разморозки заказа на разбор
    /// </summary>
    public sealed record UnfreezeReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        public UnfreezeCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
            };
        }
    }
}