namespace Faryma.Composer.Core.Features.ComposerStreamFeature.Commands
{
    /// <summary>
    /// Команда запуска стрима
    /// </summary>
    public sealed record StartCommand
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}