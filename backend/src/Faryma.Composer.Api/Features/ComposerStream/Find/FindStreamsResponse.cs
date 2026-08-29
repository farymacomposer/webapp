using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ComposerStream.Find
{
    /// <summary>
    /// Ответ на запрос поиска стримов
    /// </summary>
    public sealed record FindStreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}
