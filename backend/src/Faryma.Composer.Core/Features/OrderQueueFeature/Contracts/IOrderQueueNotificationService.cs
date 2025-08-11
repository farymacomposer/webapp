using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyNewOrderAdded(TimeSpan positionsHashCode, OrderPosition position);
        Task NotifyOrderPositionChanged(TimeSpan positionsHashCode, OrderPosition position, OrderQueueUpdateType updateType);
        Task NotifyOrderPositionsChanged(TimeSpan positionsHashCode, IEnumerable<OrderPosition> positions);
        Task NotifyOrderRemoved(TimeSpan positionsHashCode, OrderPosition position);
    }
}