using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Core.Features.ReviewFeature
{
    public sealed class ReviewService(
        UnitOfWork uow,
        OrderQueueService orderQueueService)
    {
    }
}