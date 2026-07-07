using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.FindLiveAndPlanned
{
    /// <summary>
    /// Ответ на запрос текущего и запланированных стримов
    /// </summary>
    public sealed record FindLiveAndPlannedStreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        [Required]
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}
