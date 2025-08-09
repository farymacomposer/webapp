namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Unfreeze
{
    /// <summary>
    /// Ответ на запрос разморозки заказа
    /// </summary>
    public sealed record UnfreezeReviewOrderResponse
    {
        /// <summary>
        /// Id размороженного заказа
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}