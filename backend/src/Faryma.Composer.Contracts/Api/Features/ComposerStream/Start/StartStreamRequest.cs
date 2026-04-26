namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.Start
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
