using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.Create
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
