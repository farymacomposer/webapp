namespace Faryma.Composer.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда отмены заказа
    /// </summary>
    public sealed record CancelCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Причина отмены заказа
        /// </summary>
        public required string CancelReason { get; init; }
    }
}
