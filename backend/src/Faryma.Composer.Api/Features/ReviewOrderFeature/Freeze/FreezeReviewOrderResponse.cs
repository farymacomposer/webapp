namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze
{
    /// <summary>
    /// Ответ на запрос заморозки заказа
    /// </summary>
    public sealed record FreezeReviewOrderResponse
    {
        /// <summary>
        /// Id замороженного заказа
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}