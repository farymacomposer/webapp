namespace Faryma.Composer.Application.Features.ReviewOrder
{
    /// <summary>
    /// Команда создания заказа по существующему жетону пользователя
    /// </summary>
    public sealed record CreateTokenOrderCommand
    {
        /// <summary>
        /// Id жетона пользователя
        /// </summary>
        public required long UserEntitlementId { get; init; }
    }
}
