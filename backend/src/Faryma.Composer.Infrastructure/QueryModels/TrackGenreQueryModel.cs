namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Музыкальный жанр трека
    /// </summary>
    public sealed record TrackGenreQueryModel
    {
        /// <summary>
        /// Название жанра
        /// </summary>
        public required string Name { get; init; }
    }
}