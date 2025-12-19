using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Contracts;

namespace Faryma.Composer.Application.Features.ReviewFeature
{
    public sealed class ReviewService(
        UnitOfWork uow,
        OrderQueueService orderQueueService)
    {
    }
}