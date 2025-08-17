namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Complete
{
    /// <summary>
    /// Запрос завершения стрима
    /// </summary>
    public sealed record CompleteStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; set; }
    }
}