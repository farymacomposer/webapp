using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature
{
    /// <summary>
    /// Ответ на запрос отмены стрима
    /// </summary>
    public sealed record StreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}