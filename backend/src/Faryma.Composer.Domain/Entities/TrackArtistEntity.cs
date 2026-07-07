using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Исполнитель музыкального трека
    /// </summary>
    public sealed class TrackArtistEntity : BaseEntity
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
        public ICollection<UserEntity> Users { get; set; } = [];

        /// <summary>
        /// Связь с треками
        /// </summary>
        public ICollection<TrackEntity> Tracks { get; set; } = [];
    }
}
