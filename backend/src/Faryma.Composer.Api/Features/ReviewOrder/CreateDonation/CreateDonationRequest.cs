using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Api.Features.ReviewOrder.CreateDonation
{
    /// <summary>
    /// Запрос создания донатного заказа на разбор
    /// </summary>
    public sealed record CreateDonationRequest : CreateRequestBase
    {
        /// <summary>
        /// Сумма платежа
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Сумма платежа должна быть больше 0")]
        public required long PaymentAmount { get; init; }

        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        [EnumDataType(typeof(AccountTopUpProvider), ErrorMessage = "Не задан провайдер/канал пополнения счета пользователя")]
        public required AccountTopUpProvider TopUpProvider { get; init; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (ValidationResult validationResult in base.Validate(validationContext))
            {
                yield return validationResult;
            }

            if (TopUpProvider == AccountTopUpProvider.Unspecified)
            {
                yield return new ValidationResult("Не задан провайдер/канал пополнения счета пользователя");
            }
        }
    }
}
