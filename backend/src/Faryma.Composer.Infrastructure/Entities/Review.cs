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
        public required int Rating { get; set; }

        /// <summary>
        /// Комментарий
        /// </summary>
        public required string Comment { get; set; }

        /// <summary>
        /// Дата и время завершения разбора трека
        /// </summary>
        public required DateTime CompletedAt { get; set; }

        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        public required DateTime UpdatedAt { get; set; }

        public long ReviewOrderId { get; set; }
        public long TrackId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Связанный заказ
        /// </summary>
        [ForeignKey(nameof(ReviewOrderId))]
        public required ReviewOrder ReviewOrder { get; set; }

        /// <summary>
        /// Разбираемый трек
        /// </summary>
        [ForeignKey(nameof(TrackId))]
        public required Track Track { get; set; }
    }
}