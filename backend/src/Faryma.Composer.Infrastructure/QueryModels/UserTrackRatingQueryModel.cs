namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Пользовательская оценка трека
    /// </summary>
    public sealed record UserTrackRatingQueryModel
    {
        /// <summary>
        /// Оценка
        /// </summary>
        public required int RatingValue { get; init; }
    }
}