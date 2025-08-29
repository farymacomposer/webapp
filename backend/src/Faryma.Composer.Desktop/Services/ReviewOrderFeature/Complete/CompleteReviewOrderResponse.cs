using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.Complete
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