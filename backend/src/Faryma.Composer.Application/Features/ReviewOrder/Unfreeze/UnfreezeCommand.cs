using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Unfreeze
{
    /// <summary>
    /// Команда разморозки заказа
    /// </summary>
    public sealed record UnfreezeCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}
