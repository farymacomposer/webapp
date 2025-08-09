namespace Faryma.Composer.Core.Features.ComposerStreamFeature.Commands
{
    /// <summary>
    /// Команда отмены стрима
    /// </summary>
    public sealed record CancelCommand
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}