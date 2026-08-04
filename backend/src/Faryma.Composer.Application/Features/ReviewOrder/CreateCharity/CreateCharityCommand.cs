using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.CreateCharity
{
    /// <summary>
    /// Команда создания благотворительного заказа
    /// </summary>
    public sealed record CreateCharityCommand : CreateCommandBase, IRequest<ReviewOrderEntity>
    {
    }
}
