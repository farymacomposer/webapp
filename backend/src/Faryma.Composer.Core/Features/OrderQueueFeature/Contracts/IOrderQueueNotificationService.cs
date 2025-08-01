using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(OrderPosition orderPosition);
        Task NotifyOrderPositionChanged(OrderPosition orderPosition);
        Task NotifyOrderRemoved(OrderPosition orderPosition);
        Task NotifyReviewStarted(ReviewOrder reviewOrder);
        Task NotifyReviewCompleted(ReviewOrder reviewOrder);
    }
}