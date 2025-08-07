namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Запрос поднятия заказа в очереди
    /// </summary>
    public sealed record UpCommand
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string Nickname { get; init; }

        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required decimal PaymentAmount { get; init; }
    }
}