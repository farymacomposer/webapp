using System.ComponentModel.DataAnnotations;
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
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}