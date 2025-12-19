namespace Faryma.Composer.Application.Features.ReviewOrderFeature.Commands
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
        public required decimal PaymentAmount { get; init; }
    }
}