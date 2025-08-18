using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Complete
{
    /// <summary>
    /// Ответ на запрос выполнения заказа
    /// </summary>
    public sealed record CompleteReviewOrderResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }

        /// <summary>
        /// Id созданного разбора
        /// </summary>
        public required long ReviewId { get; init; }
    }
}