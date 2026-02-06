using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Результат разбора трека композитором
    /// </summary>
    [Table("reviews")]
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

        public long? ReviewOrderId { get; set; }
        public long? TrackId { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, создавший разбор
        /// </summary>
        [ForeignKey(nameof(CreatedByUserId))]
        public required UserEntity CreatedByUser { get; set; }

        /// <summary>
        /// Связанный заказ
        /// </summary>
        [ForeignKey(nameof(ReviewOrderId))]
        public ReviewOrderEntity? ReviewOrder { get; set; }

        /// <summary>
        /// Связанный музыкальный трек
        /// </summary>
        [ForeignKey(nameof(TrackId))]
        public TrackEntity? Track { get; set; }
    }
}