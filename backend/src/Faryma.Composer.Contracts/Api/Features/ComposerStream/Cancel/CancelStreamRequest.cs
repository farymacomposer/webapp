namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.Cancel
{
    /// <summary>
    /// Запрос отмены стрима
    /// </summary>
    public sealed record CancelStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}