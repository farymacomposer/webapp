using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create
{
    /// <summary>
    /// Ответ на запрос создания заказа на разбор
    /// </summary>
    public sealed record CreateReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}