using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.FindLiveAndPlanned
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