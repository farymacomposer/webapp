using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Результат разбора трека композитором
    /// </summary>
    public sealed class Review : BaseEntity
    {
        /// <summary>
        /// Оценка
        /// </summary>
        public required int RatingValue { get; set; }

        /// <summary>
        /// Дата и время создания разбора
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        public required DateTime UpdatedAt { get; set; }

        public long? ReviewOrderId { get; set; }
        public long? TrackId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Связанный заказ
        /// </summary>
        [ForeignKey(nameof(ReviewOrderId))]
        public ReviewOrder? ReviewOrder { get; set; }

        /// <summary>
        /// Связанный музыкальный трек
        /// </summary>
        [ForeignKey(nameof(TrackId))]
        public Track? Track { get; set; }
    }
}