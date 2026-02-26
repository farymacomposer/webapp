using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Unfreeze
{
    /// <summary>
    /// Ответ на запрос разморозки заказа
    /// </summary>
    public sealed record UnfreezeReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}