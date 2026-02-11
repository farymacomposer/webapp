using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Пользовательская оценка трека
    /// </summary>
    public sealed class UserTrackRatingEntity : BaseEntity
    {
        /// <summary>
        /// Оценка
        /// </summary>
        public required int RatingValue { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Дата и время создания оценки
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обновления оценки
        /// </summary>
        public required DateTime UpdatedAt { get; set; }

        public long TrackId { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Оцениваемый трек
        /// </summary>
        public required TrackEntity Track { get; set; }

        /// <summary>
        /// Пользователь, создавший оценку
        /// </summary>
        public required UserEntity CreatedByUser { get; set; }
    }
}