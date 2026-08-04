using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateFree
{
    /// <summary>
    /// Команда создания бесплатного заказа
    /// </summary>
    public sealed record CreateFreeCommand : CreateCommandBase, IRequest<ReviewOrderEntity>
    {
    }
}
