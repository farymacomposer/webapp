using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Cancel
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