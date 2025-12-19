namespace Faryma.Composer.Contracts.Api.Features.ComposerStream.Complete
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