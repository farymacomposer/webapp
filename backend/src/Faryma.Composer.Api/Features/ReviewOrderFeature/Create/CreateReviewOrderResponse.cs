namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Create
{
    /// <summary>
    /// Ответ на запрос создания заказа на разбор
    /// </summary>
    public sealed class CreateReviewOrderResponse
    {
        /// <summary>
        /// Id заказа на разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}