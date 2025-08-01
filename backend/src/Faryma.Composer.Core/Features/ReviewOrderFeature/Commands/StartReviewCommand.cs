namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда начала разбора трека
    /// </summary>
    public sealed record StartReviewCommand
    {
        /// <summary>
        /// ID заказа разбора трека
        /// </summary>
        public long ReviewOrderId { get; init; }
    }
}