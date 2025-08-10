namespace Faryma.Composer.Core.Features.ComposerStreamFeature.Commands
{
    /// <summary>
    /// Команда завершения стрима
    /// </summary>
    public sealed record CompleteCommand
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}