using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.Contracts
{
    public interface INotificationService
    {
        Task SendOrderPosition(OrderPositionTracker orderPositionTracker);
    }
}