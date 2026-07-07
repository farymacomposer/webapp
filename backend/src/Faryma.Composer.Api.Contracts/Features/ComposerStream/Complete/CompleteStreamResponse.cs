using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.Complete
{
    /// <summary>
    /// Ответ на запрос завершения стрима
    /// </summary>
    public sealed record CompleteStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}
