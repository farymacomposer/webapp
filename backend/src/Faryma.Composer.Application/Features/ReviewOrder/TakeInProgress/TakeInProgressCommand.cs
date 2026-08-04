using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Команда взятия заказа в работу
    /// </summary>
    public sealed record TakeInProgressCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}
