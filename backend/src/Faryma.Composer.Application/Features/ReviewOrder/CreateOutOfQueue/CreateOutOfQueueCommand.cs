using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateOutOfQueue
{
    /// <summary>
    /// Команда создания заказа вне очереди
    /// </summary>
    public sealed record CreateOutOfQueueCommand : CreateCommandBase, IRequest<ReviewOrderEntity>
    {
    }
}
