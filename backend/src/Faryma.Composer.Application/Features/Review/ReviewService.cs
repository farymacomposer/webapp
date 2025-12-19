using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.Review
{
    public sealed class ReviewService(
        UnitOfWork uow,
        OrderQueueService orderQueueService)
    {
    }
}