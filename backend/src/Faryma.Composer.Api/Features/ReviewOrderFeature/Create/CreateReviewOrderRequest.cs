using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.Create
{
    /// <summary>
    /// Запрос создания заказа на разбор
    /// </summary>
    public sealed record CreateReviewOrderRequest : IValidatableObject
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Длина псевдонима должна быть в пределах от 1 до 40 символов")]
        public required string Nickname { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        [EnumDataType(typeof(ReviewOrderType), ErrorMessage = "Недопустимый тип заказа")]
        public required ReviewOrderType OrderType { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [Url(ErrorMessage = "Некорректная ссылка на трек")]
        public string? TrackUrl { get; init; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public decimal? PaymentAmount { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        [StringLength(200, ErrorMessage = "Максимальная длина комментария - 200 символов")]
        public string? UserComment { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrderType is not (
                ReviewOrderType.Donation
                or ReviewOrderType.OutOfQueue
                or ReviewOrderType.Free
                or ReviewOrderType.Charity))
            {
                yield return new ValidationResult($"Тип заказа не поддерживается `{OrderType}`");
            }

            if (PaymentAmount < 0)
            {
                yield return new ValidationResult("Сумма платежа не может быть отрицательной");
            }

            if (OrderType == ReviewOrderType.Donation && PaymentAmount == 0)
            {
                yield return new ValidationResult("Для донатных заказов сумма платежа не может быть равна нулю");
            }

            if (OrderType
                is ReviewOrderType.Free
                or ReviewOrderType.OutOfQueue
                or ReviewOrderType.Charity
                && PaymentAmount > 0)
            {
                yield return new ValidationResult("Для бесплатных, внеочередных и благотворительных заказов сумма платежа должна быть равна нулю");
            }
        }
    }
}