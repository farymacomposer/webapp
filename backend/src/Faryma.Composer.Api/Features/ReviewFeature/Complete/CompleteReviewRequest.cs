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
        [Required]
        public required long ReviewOrderId { get; set; }

        /// <summary>
        /// Оценка трека (1-10)
        /// </summary>
        [Range(1, 10, ErrorMessage = "Оценка должна быть от 1 до 10")]
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