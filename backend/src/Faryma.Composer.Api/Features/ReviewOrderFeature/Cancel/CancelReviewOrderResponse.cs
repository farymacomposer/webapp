namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Cancel
{
    /// <summary>
    /// Ответ на запрос отмены заказа на разбор
    /// </summary>
    public sealed record CancelReviewOrderResponse
    {
        /// <summary>
        /// Id отмененного заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}