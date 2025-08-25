namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
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
    }
}