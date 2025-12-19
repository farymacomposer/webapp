namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Freeze
{
    /// <summary>
    /// Запрос заморозки заказа
    /// </summary>
    public sealed record FreezeReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}