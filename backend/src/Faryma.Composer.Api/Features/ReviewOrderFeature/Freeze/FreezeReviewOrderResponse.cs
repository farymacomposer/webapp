using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Shared.Dto;

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
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}