namespace Faryma.Composer.Core.Features.ComposerStreamFeature.Commands
{
    /// <summary>
    /// Команда отмены стрима
    /// </summary>
    public sealed record CancelCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}