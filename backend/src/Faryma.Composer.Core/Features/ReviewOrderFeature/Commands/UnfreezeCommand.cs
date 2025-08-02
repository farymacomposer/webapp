namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда разморозки заказа разбора трека
    /// </summary>
    public sealed record UnfreezeCommand
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}