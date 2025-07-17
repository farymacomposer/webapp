using Faryma.Composer.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Исполнитель музыкального трека
    /// </summary>
    [Index(nameof(NormalizedName), IsUnique = true)]
    public sealed class Artist : BaseEntity
    {
        /// <summary>
        /// Имя исполнителя
        /// </summary>
        public required string Name { get; set; }

        public required string NormalizedName { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Связь с пользователями
        /// </summary>
        public ICollection<User> Users { get; set; } = [];

        /// <summary>
        /// Связь с треками
        /// </summary>
        public ICollection<Track> Tracks { get; set; } = [];
    }
}