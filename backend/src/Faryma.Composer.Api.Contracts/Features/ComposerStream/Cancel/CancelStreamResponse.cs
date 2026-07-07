using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.Cancel
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
