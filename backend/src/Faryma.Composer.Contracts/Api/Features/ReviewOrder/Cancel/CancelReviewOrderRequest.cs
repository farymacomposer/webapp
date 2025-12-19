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
    }
}