using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.Freeze
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