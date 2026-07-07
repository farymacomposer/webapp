using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.Cancel
{
    /// <summary>
    /// Ответ на запрос отмены заказа
    /// </summary>
    public sealed record CancelReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
