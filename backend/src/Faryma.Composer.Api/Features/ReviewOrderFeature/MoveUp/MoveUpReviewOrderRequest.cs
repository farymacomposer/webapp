using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.MoveUp
{
    /// <summary>
    /// Запрос поднятия заказа в очереди
    /// </summary>
    public sealed record MoveUpReviewOrderRequest : IValidatableObject
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Длина псевдонима должна быть в пределах от 1 до 40 символов")]
        public required string Nickname { get; init; }

        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required decimal PaymentAmount { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentAmount <= 0)
            {
                yield return new ValidationResult("Сумма платежа должна быть больше нуля");
            }
        }
    }
}