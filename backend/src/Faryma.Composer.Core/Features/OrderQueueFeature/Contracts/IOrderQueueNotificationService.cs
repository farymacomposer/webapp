using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(int positionsHashCode, OrderPosition position);
        Task NotifyOrderPositionChanged(int positionsHashCode, OrderPosition position, OrderQueueUpdateType updateType);
        Task NotifyOrderPositionsChanged(int positionsHashCode, IEnumerable<OrderPosition> positions);
        Task NotifyOrderRemoved(int positionsHashCode, OrderPosition position);
    }
}