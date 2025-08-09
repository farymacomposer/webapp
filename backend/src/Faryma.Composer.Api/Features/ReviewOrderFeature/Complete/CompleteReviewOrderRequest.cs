using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Complete
{
    /// <summary>
    /// Запрос выполнения заказа
    /// </summary>
    public sealed record CompleteReviewOrderRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        [Range(0, 26, ErrorMessage = "Оценка должна быть от 0 до 26")]
        public required int Rating { get; set; }

        /// <summary>
        /// Комментарий к разбору
        /// </summary>
        [Required]
        [StringLength(300, MinimumLength = 1, ErrorMessage = "Обязательно наличие комментария, но не более 300 символов")]
        public required string Comment { get; set; }
    }
}