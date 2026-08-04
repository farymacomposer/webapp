using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.Cancel
{
    /// <summary>
    /// Команда отмены заказа
    /// </summary>
    public sealed record CancelCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Причина отмены заказа
        /// </summary>
        public required string CancelReason { get; init; }
    }
}
