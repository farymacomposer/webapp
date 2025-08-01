using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewFeature.Complete
{
    /// <summary>
    /// Запрос завершения разбора трека
    /// </summary>
    public sealed record CompleteReviewRequest : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Rating < 1 || Rating > 10)
            {
                yield return new ValidationResult("Оценка должна быть от 1 до 10");
            }

            if (string.IsNullOrWhiteSpace(Comment))
            {
                yield return new ValidationResult("Комментарий не может быть пустым");
            }
        }

        public Core.Features.ReviewFeature.Commands.CompleteReviewCommand Map()
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