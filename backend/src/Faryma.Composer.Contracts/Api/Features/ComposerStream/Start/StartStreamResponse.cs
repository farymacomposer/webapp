using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.Start
{
    /// <summary>
    /// Ответ на запрос запуска стрима
    /// </summary>
    public sealed record StartStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}