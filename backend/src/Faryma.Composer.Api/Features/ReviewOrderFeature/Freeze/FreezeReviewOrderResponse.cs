namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze
{
    /// <summary>
    /// Ответ на запрос заморозки заказа на разбор
    /// </summary>
    public sealed record FreezeReviewOrderResponse
    {
        /// <summary>
        /// Id замороженного заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}