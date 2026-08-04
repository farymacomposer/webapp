using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateDonation
{
    /// <summary>
    /// Команда создания донатного заказа
    /// </summary>
    public sealed record CreateDonationCommand : CreateCommandBase, IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required long PaymentAmount { get; init; }

        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        public required AccountTopUpProvider TopUpProvider { get; init; }
    }
}
