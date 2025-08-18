namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Результат разбора трека композитором
    /// </summary>
    public sealed record ReviewQueryModel
    {
        /// <summary>
        /// Оценка
        /// </summary>
        public required int Rating { get; init; }
    }
}