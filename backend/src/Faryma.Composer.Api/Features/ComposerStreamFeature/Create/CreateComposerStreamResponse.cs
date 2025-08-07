using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Create
{
    /// <summary>
    /// Ответ на запрос создания стрима
    /// </summary>
    public sealed record CreateComposerStreamResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}