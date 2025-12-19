using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Api.Shared.Dto
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed record ComposerStreamDto
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long Id { get; init; }

        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; init; }

        /// <summary>
        /// Статус стрима
        /// </summary>
        public required ComposerStreamStatus Status { get; init; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        public required ComposerStreamType Type { get; init; }

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public required DateTime? StartedAt { get; init; }

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public required DateTime? CompletedAt { get; init; }

        public static ComposerStreamDto Map(ComposerStreamEntity item)
        {
            return new()
            {
                Id = item.Id,
                EventDate = item.EventDate,
                Status = item.Status,
                Type = item.Type,
                StartedAt = item.StartedAt,
                CompletedAt = item.CompletedAt,
            };
        }
    }
}