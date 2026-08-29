using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ComposerStream.Create
{
    /// <summary>
    /// Ответ на запрос создания стрима
    /// </summary>
    public sealed record CreateResponse
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        [Required]
        public required ComposerStreamDto ComposerStream { get; init; }
    }
}
