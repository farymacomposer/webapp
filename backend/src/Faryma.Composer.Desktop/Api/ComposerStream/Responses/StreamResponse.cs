using Faryma.Composer.Desktop.Api.Dto;

namespace Faryma.Composer.Desktop.Api.ComposerStream.Responses
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed record StreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}