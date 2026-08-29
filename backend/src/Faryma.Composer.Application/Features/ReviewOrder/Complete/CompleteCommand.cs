using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Complete
{
    /// <summary>
    /// Команда выполнения заказа
    /// </summary>
    public sealed record CompleteCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        public required int Rating { get; init; }
    }
}
