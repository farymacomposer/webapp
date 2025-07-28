using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Up
{
    /// <summary>
    /// Запрос поднятия заказа в очереди
    /// </summary>
    public sealed record UpReviewOrderRequest : IValidatableObject
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [Required]
        public required string Nickname { get; set; }

        /// <summary>
        /// Id заказа на разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required decimal PaymentAmount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentAmount == 0)
            {
                yield return new ValidationResult("Сумма платежа не может быть равна нулю");
            }
        }

        public UpCommand Map()
        {
            return new()
            {
                Nickname = Nickname,
                ReviewOrderId = ReviewOrderId,
                PaymentAmount = PaymentAmount,
            };
        }
    }
}