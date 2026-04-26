namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Unfreeze
{
    /// <summary>
    /// Запрос разморозки заказа
    /// </summary>
    public sealed record UnfreezeReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}
