using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Страна производства музыкального трека
    /// </summary>
    public sealed class TrackCountryEntity : BaseEntity
    {
        /// <summary>
        /// Название страны
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Связь с треками
        /// </summary>
        public ICollection<TrackEntity> Tracks { get; set; } = [];
    }
}
