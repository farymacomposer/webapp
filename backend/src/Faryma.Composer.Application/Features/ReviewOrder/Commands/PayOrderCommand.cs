using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда оплаты заказа разбора трека
    /// </summary>
    public sealed record PayOrderCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Псевдоним плательщика
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

        /// <summary>
        /// Id пользователя, создавшего платеж
        /// </summary>
        public required Guid CreatedByUserId { get; init; }
    }
}
