using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.Complete
{
    /// <summary>
    /// Ответ на запрос завершения стрима
    /// </summary>
    public sealed record CompleteStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}