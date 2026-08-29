using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ComposerStream.FindLiveAndPlanned
{
    /// <summary>
    /// Ответ на запрос текущего и запланированных стримов
    /// </summary>
    public sealed record FindLiveAndPlannedResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}
