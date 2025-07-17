using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    [Index(nameof(EventDate), IsUnique = true)]
    public sealed class ComposerStream : BaseEntity
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; set; }

        /// <summary>
        /// Статус стрима
        /// </summary>
        public required ComposerStreamStatus Status { get; set; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        public required ComposerStreamType Type { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Заказы разборов треков
        /// </summary>
        public ICollection<ReviewOrder> ReviewOrders { get; set; } = [];
    }
}