namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.Complete
{
    /// <summary>
    /// Запрос выполнения заказа
    /// </summary>
    public sealed record CompleteReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        public required int Rating { get; init; }
    }
}