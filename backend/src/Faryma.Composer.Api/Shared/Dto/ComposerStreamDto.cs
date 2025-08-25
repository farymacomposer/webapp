using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Shared.Dto
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
        public required DateTime? WentLiveAt { get; init; }

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public required DateTime? CompletedAt { get; init; }

        public static ComposerStreamDto Map(ComposerStream item)
        {
            return new()
            {
                Id = item.Id,
                EventDate = item.EventDate,
                Status = item.Status,
                Type = item.Type,
                WentLiveAt = item.WentLiveAt,
                CompletedAt = item.CompletedAt,
            };
        }
    }
}