namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Cancel
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