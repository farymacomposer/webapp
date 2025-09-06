using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.MoveUp
{
    /// <summary>
    /// Ответ на запрос поднятия заказа в очереди
    /// </summary>
    public sealed record MoveUpReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }

        /// <summary>
        /// Id платежа
        /// </summary>
        public required long PaymentTransactionId { get; init; }
    }
}