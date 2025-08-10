using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Find
{
    /// <summary>
    /// Ответ на запрос поиска стримов
    /// </summary>
    public sealed record FindComposerStreamResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}