using Faryma.Composer.Contracts.Api.Shared.Dto;

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
