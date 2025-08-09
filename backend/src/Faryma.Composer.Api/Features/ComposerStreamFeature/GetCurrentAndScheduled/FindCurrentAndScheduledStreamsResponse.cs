using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.GetCurrentAndScheduled
{
    public sealed class FindCurrentAndScheduledStreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Items { get; init; }
    }
}