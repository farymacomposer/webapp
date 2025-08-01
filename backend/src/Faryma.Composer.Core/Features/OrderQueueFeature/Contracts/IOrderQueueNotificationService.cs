using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(OrderPosition orderPosition);
        Task NotifyOrderPositionChanged(OrderPosition orderPosition);
        Task NotifyOrderRemoved(OrderPosition orderPosition);
        Task NotifyReviewStarted(OrderPosition orderPosition);
        Task NotifyReviewCompleted(OrderPosition orderPosition);
    }
}