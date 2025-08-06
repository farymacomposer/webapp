using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewFeature.Complete
{
    /// <summary>
    /// Запрос завершения разбора трека
    /// </summary>
    public sealed record CompleteReviewRequest
    {
        /// <summary>
        /// ID заказа разбора трека
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
        [StringLength(300, MinimumLength = 1, ErrorMessage = "Обязательно наличие комментария, но не более 300 символов")]
        public required string Comment { get; set; }

        public CompleteReviewCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
                Rating = Rating,
                Comment = Comment.Trim(),
            };
        }
    }
}