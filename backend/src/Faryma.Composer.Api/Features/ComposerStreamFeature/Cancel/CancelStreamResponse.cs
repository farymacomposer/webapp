using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Shared.Dto;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Cancel
{
    /// <summary>
    /// Ответ на запрос отмены стрима
    /// </summary>
    public sealed record CancelStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}