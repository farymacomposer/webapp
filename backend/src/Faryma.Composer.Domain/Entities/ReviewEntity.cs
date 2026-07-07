using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Результат разбора трека композитором
    /// </summary>
    public sealed class ReviewEntity : BaseEntity
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

        /// <summary>
        /// Ссылка на видео разбора
        /// </summary>
        public string? TimestampUrl { get; set; }

        public long? ReviewOrderId { get; set; }
        public long? TrackId { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, создавший разбор
        /// </summary>
        public required UserEntity CreatedByUser { get; set; }

        /// <summary>
        /// Связанный заказ
        /// </summary>
        public ReviewOrderEntity? ReviewOrder { get; set; }

        /// <summary>
        /// Связанный музыкальный трек
        /// </summary>
        public TrackEntity? Track { get; set; }
    }
}
