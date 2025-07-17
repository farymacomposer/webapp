namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Up
{
    /// <summary>
    /// Ответ на запрос поднятия заказа в очереди
    /// </summary>
    public sealed class UpReviewOrderResponse
    {
        /// <summary>
        /// Id платежа
        /// </summary>
        public required long PaymentTransactionId { get; init; }
    }
}