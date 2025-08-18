namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Страна производства музыкального трека
    /// </summary>
    public sealed record TrackCountryQueryModel
    {
        /// <summary>
        /// Название страны
        /// </summary>
        public required string Name { get; init; }
    }
}