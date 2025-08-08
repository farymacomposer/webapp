using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed record ComposerStreamDto
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long Id { get; set; }

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

        public static ComposerStreamDto Map(ComposerStream item)
        {
            return new()
            {
                Id = item.Id,
                EventDate = item.EventDate,
                Status = item.Status,
                Type = item.Type,
            };
        }
    }
}