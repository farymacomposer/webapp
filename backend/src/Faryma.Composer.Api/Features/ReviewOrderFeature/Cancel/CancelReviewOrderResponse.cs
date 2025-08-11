using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Cancel
{
    /// <summary>
    /// Ответ на запрос отмены заказа
    /// </summary>
    public sealed record CancelReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}