using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Unfreeze
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