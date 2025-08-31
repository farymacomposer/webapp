using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Find
{
    /// <summary>
    /// Ответ на запрос поиска стримов
    /// </summary>
    public sealed record FindComposerStreamResponse
    {
        /// <summary>
        /// Список стримов
        /// </summary>
        public required IEnumerable<ComposerStreamDto> Streams { get; init; }
    }
}