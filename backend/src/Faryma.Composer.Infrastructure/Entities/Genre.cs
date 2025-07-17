using Faryma.Composer.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Музыкальный жанр
    /// </summary>
    [Index(nameof(NormalizedName), IsUnique = true)]
    public sealed class Genre : BaseEntity
    {
        /// <summary>
        /// Название жанра
        /// </summary>
        public required string Name { get; set; }

        public required string NormalizedName { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Связь с треками
        /// </summary>
        public ICollection<Track> Tracks { get; set; } = [];
    }
}