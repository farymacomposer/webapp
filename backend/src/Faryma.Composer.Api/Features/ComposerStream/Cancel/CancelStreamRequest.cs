using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Contracts.Features.ComposerStream.Cancel
{
    /// <summary>
    /// Запрос отмены стрима
    /// </summary>
    public sealed record CancelStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id стрима должен быть больше нуля")]
        public required long ComposerStreamId { get; init; }
    }
}
