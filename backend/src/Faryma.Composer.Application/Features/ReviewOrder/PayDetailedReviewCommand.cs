using Faryma.Composer.Domain.Enums;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    /// <summary>
    /// Команда оплаты подробного разбора заказа
    /// </summary>
    public sealed record PayDetailedReviewCommand : IRequest<PayDetailedReviewResult>
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
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        public AccountTopUpProvider? TopUpProvider { get; init; }

        /// <summary>
        /// Id жетона на подробный разбор
        /// </summary>
        public long? UserEntitlementId { get; init; }

        /// <summary>
        /// Id пользователя, оформившего оплату
        /// </summary>
        public required Guid CreatedByUserId { get; init; }
    }
}
