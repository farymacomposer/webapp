using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.MoveUp
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