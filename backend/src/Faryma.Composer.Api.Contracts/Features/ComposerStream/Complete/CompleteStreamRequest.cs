using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.Complete
{
    /// <summary>
    /// Запрос завершения стрима
    /// </summary>
    public sealed record CompleteStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id стрима должен быть больше нуля")]
        public required long ComposerStreamId { get; init; }
    }
}
