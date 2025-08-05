namespace Faryma.Composer.Core.Features.ReviewFeature.Commands
{
    /// <summary>
    /// Команда завершения разбора трека
    /// </summary>
    public sealed record CompleteReviewCommand
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        public required int Rating { get; init; }

        /// <summary>
        /// Комментарий к разбору
        /// </summary>
        public required string Comment { get; init; }
    }
}