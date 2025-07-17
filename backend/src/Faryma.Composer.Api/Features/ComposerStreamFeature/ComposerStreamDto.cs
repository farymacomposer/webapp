using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed class ComposerStreamDto
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
    }
}