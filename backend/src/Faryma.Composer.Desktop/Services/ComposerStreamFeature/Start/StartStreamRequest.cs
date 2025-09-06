namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Start
{
    /// <summary>
    /// Запрос запуска стрима
    /// </summary>
    public sealed record StartStreamRequest
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}