using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Start
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