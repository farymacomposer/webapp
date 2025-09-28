namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Requests
{
    /// <summary>
    /// Запрос завершения стрима
    /// </summary>
    public sealed record CompleteStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}