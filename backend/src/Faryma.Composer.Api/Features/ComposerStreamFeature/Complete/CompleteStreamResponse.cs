using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Complete
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