using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.GetCurrentAndScheduled
{
    /// <summary>
    /// Ответ на запрос текущего и запланированных стримов
    /// </summary>
    public sealed record FindCurrentAndScheduledStreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}