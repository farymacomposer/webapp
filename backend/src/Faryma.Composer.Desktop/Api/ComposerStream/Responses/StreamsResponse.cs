using Faryma.Composer.Desktop.Api.Dto;

namespace Faryma.Composer.Desktop.Api.ComposerStream.Responses
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