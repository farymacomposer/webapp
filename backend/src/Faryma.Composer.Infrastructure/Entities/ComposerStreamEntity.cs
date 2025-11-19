using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    [Index(nameof(EventDate), IsUnique = true)]
    public sealed class ComposerStreamEntity : BaseEntity
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; set; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        [Column(TypeName = DbContextHelper.ComposerStreamTypeEnum)]
        public required ComposerStreamType Type { get; set; }

        /// <summary>
        /// Статус стрима
        /// </summary>
        [Column(TypeName = DbContextHelper.ComposerStreamStatusEnum)]
        public required ComposerStreamStatus Status { get; set; }

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Заказы, созданные в этом стриме
        /// </summary>
        public ICollection<ReviewOrderEntity> CreatedReviewOrders { get; set; } = [];

        /// <summary>
        /// Заказы, взятые в работу в этом стриме
        /// </summary>
        public ICollection<ReviewOrderEntity> ProcessedReviewOrders { get; set; } = [];
    }
}