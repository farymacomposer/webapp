using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Freeze
{
    /// <summary>
    /// Команда заморозки заказа
    /// </summary>
    public sealed record FreezeCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}
