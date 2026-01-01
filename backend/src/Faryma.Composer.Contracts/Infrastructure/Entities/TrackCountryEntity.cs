using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Страна производства музыкального трека
    /// </summary>
    [Table("track_countries")]
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