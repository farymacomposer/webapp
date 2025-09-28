using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Responses
{
    /// <summary>
    /// Список стримов
    /// </summary>
    public sealed record StreamsResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}