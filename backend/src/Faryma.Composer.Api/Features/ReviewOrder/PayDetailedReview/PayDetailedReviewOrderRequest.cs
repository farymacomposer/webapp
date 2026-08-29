using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Api.Features.ReviewOrder.PayDetailedReview
{
    /// <summary>
    /// Запрос оплаты подробного разбора заказа
    /// </summary>
    public sealed record PayDetailedReviewOrderRequest : IValidatableObject
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id заказа должен быть больше нуля")]
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Длина псевдонима должна быть в пределах от 1 до 40 символов")]
        public required string Nickname { get; init; }

        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        [EnumDataType(typeof(AccountTopUpProvider), ErrorMessage = "Недопустимый провайдер/канал пополнения счета пользователя")]
        public AccountTopUpProvider? TopUpProvider { get; init; }

        /// <summary>
        /// Id жетона на подробный разбор
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id жетона должен быть больше нуля")]
        public long? UserEntitlementId { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((TopUpProvider is null) == (UserEntitlementId is null))
            {
                yield return new ValidationResult("Укажите либо платеж, либо жетон подробного разбора");
            }

            if (TopUpProvider is not null and not (
                AccountTopUpProvider.Donationalerts
                or AccountTopUpProvider.Donatty
                or AccountTopUpProvider.TwitchChannelPoints
                or AccountTopUpProvider.Manual))
            {
                yield return new ValidationResult($"Пополнения счета через '{TopUpProvider}' не поддерживается");
            }
        }
    }
}
