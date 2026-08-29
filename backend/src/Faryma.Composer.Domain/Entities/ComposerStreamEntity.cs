using Faryma.Composer.Domain.Entities.Abstractions;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Domain.Entities
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

        public void Start(DateTime now)
        {
            if (Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{Status}'", this);
            }

            Status = ComposerStreamStatus.Live;
            StartedAt = now;
        }

        public void Complete(DateTime now)
        {
            if (Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{Status}'", this);
            }

            Status = ComposerStreamStatus.Completed;
            CompletedAt = now;
        }

        public void Cancel()
        {
            if (Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно отменить стрим в статусе '{Status}'", this);
            }

            Status = ComposerStreamStatus.Canceled;
        }
    }
}
