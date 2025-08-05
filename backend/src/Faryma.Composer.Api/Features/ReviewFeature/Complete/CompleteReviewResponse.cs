namespace Faryma.Composer.Api.Features.ReviewFeature.Complete
{
    /// <summary>
    /// Ответ на запрос завершения разбора трека
    /// </summary>
    public sealed record CompleteReviewResponse
    {
        /// <summary>
        /// ID завершенного заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// ID созданного разбора
        /// </summary>
        public required long ReviewId { get; init; }
    }
}