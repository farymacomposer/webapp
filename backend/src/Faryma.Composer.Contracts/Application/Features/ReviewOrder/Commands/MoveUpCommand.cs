using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда поднятия заказа в очереди
    /// </summary>
    public sealed record MoveUpCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string Nickname { get; init; }

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