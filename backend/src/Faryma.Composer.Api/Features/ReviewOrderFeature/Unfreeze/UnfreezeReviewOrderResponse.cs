namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Unfreeze
{
    /// <summary>
    /// Ответ на запрос разморозки заказа на разбор
    /// </summary>
    public sealed record UnfreezeReviewOrderResponse
    {
        /// <summary>
        /// ID размороженного заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}