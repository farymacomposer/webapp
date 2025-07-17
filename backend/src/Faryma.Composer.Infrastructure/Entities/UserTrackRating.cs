using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Пользовательская оценка трека
    /// </summary>
    public sealed class UserTrackRating : BaseEntity
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
        public Guid UserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Оцениваемый трек
        /// </summary>
        [ForeignKey(nameof(TrackId))]
        public required Track Track { get; set; }

        /// <summary>
        /// Пользователь, оставивший оценку
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public required User User { get; set; }
    }
}