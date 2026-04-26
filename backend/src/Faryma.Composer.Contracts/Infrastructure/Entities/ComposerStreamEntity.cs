using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed class ComposerStreamEntity : BaseEntity
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; set; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        public required ComposerStreamType Type { get; set; }

        /// <summary>
        /// Статус стрима
        /// </summary>
        public required ComposerStreamStatus Status { get; set; }

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        public Guid CreatedByUserId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, создавший стрим
        /// </summary>
        public required UserEntity CreatedByUser { get; set; }

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
