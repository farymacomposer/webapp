using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(int syncVersion, OrderPosition position);
        Task NotifyOrderPositionChanged(int syncVersion, OrderPosition position, OrderQueueUpdateType updateType);
        Task NotifyOrderPositionsChanged(OrderQueue orderQueue);
        Task NotifyOrderRemoved(int syncVersion, OrderPosition position);
    }
}