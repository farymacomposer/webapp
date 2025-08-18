namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Исполнитель музыкального трека
    /// </summary>
    public sealed record TrackArtistQueryModel
    {
        /// <summary>
        /// Имя исполнителя
        /// </summary>
        public required string Name { get; init; }
    }
}