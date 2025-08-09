using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(OrderPosition position);
        Task NotifyOrderPositionChanged(OrderPosition position, OrderQueueUpdateType updateType);
        Task NotifyOrderRemoved(OrderPosition position);
    }
}