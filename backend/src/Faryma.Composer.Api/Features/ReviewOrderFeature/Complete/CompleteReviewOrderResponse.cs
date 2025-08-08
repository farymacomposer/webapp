namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Complete
{
    /// <summary>
    /// Ответ на запрос выполнения заказа
    /// </summary>
    public sealed record CompleteReviewOrderResponse
    {
        /// <summary>
        /// Id выполненного заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Id созданного разбора
        /// </summary>
        public required long ReviewId { get; init; }
    }
}