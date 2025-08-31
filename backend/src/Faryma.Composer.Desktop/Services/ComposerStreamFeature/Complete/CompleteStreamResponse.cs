using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Complete
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