namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда отмены заказа разбора трека
    /// </summary>
    public sealed record CancelCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}