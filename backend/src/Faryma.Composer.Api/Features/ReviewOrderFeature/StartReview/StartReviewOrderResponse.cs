namespace Faryma.Composer.Api.Features.ReviewOrderFeature.StartReview
{
    /// <summary>
    /// Ответ на запрос начала разбора трека
    /// </summary>
    public sealed record StartReviewOrderResponse
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}