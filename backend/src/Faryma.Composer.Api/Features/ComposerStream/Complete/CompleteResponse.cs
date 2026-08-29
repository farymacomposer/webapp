using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ComposerStream.Complete
{
    /// <summary>
    /// Ответ на запрос завершения стрима
    /// </summary>
    public sealed record CompleteResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}
