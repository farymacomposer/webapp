using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.ReviewFeature
{
    public sealed class ReviewService(
        UnitOfWork uow,
        OrderQueueService orderQueueService)
    {
    }
}