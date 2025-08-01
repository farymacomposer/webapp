namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда отмены заказа разбора трека
    /// </summary>
    public sealed record CancelCommand
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public long ReviewOrderId { get; init; }
    }
}