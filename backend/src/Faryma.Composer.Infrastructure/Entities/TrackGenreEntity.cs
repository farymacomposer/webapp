using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Музыкальный жанр трека
    /// </summary>
    public sealed class TrackGenreEntity : BaseEntity
    {
        /// <summary>
        /// Название жанра
        /// </summary>
        public required string Name { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Связь с треками
        /// </summary>
        public ICollection<TrackEntity> Tracks { get; set; } = [];
    }
}