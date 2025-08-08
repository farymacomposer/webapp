using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Find
{
    /// <summary>
    /// Ответ на запрос поиска стримов композитора
    /// </summary>
    public sealed record FindComposerStreamResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Items { get; init; }
    }
}