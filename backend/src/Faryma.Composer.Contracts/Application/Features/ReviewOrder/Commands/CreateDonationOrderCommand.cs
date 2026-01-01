using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда создания донатного заказа
    /// </summary>
    public sealed record CreateDonationOrderCommand : CreateCommandBase
    {
        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required decimal PaymentAmount { get; init; }

        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        public required AccountTopUpProvider TopUpProvider { get; init; }
    }
}