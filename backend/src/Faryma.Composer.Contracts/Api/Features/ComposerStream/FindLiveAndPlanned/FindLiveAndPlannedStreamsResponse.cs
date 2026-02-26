using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.FindLiveAndPlanned
{
    /// <summary>
    /// Ответ на запрос текущего и запланированных стримов
    /// </summary>
    public sealed record FindLiveAndPlannedStreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}