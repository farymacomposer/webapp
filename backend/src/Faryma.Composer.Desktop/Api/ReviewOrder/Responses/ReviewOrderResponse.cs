using Faryma.Composer.Desktop.Api.Shared.Dto;

namespace Faryma.Composer.Desktop.Api.ReviewOrder.Responses
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    public sealed record ReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}