using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.MoveUp
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
        public required long PaymentAmount { get; init; }

        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        [EnumDataType(typeof(AccountTopUpProvider), ErrorMessage = "Недопустимый провайдер/канал пополнения счета пользователя")]
        public required AccountTopUpProvider TopUpProvider { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentAmount <= 0)
            {
                yield return new ValidationResult("Сумма платежа должна быть больше нуля");
            }

            if (TopUpProvider is not (
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