namespace Faryma.Composer.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда создания заказа по существующему жетону пользователя
    /// </summary>
    public sealed record CreateTokenOrderCommand : CreateCommandBase
    {
        /// <summary>
        /// Id жетона пользователя
        /// </summary>
        public required long UserEntitlementId { get; init; }
    }
}
