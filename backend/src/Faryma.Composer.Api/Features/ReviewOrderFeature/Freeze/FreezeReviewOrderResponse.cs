using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze
{
    /// <summary>
    /// Ответ на запрос заморозки заказа
    /// </summary>
    public sealed record FreezeReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}