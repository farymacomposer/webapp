using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Create
{
    /// <summary>
    /// Ответ на запрос создания стрима
    /// </summary>
    public sealed record CreateStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}