using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;
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
        public required string Nickname { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        [EnumDataType(typeof(ReviewOrderType), ErrorMessage = "Недопустимый тип заказа")]
        public required ReviewOrderType OrderType { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; set; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrderType == ReviewOrderType.Donation && PaymentAmount == 0)
            {
                yield return new ValidationResult("Для донатных заказов сумма платежа не может быть равна нулю");
            }

            if (OrderType is ReviewOrderType.Free or ReviewOrderType.OutOfQueue && PaymentAmount > 0)
            {
                yield return new ValidationResult("Для бесплатных и внеочередных заказов сумма платежа должна быть равна нулю");
            }
        }

        public CreateCommand Map()
        {
            return new()
            {
                Nickname = Nickname,
                OrderType = OrderType,
                PaymentAmount = PaymentAmount,
                TrackUrl = TrackUrl,
                UserComment = UserComment,
            };
        }
    }
}