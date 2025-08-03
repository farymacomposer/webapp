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
        /// Оценка трека (1-26)
        /// </summary>
        [Range(0, 26, ErrorMessage = "Оценка должна быть от 0 до 26")]
        public required int Rating { get; set; }

        /// <summary>
        /// Комментарий к разбору
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "Комментарий не может быть пустым")]
        public required string Comment { get; set; }

        public CompleteReviewCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
                Rating = Rating,
                Comment = Comment,
            };
        }
    }
}