using Faryma.Composer.Application.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Application.Features.OrderQueueFeature.Contracts
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyQueueUpdated(OrderQueueSnapshot snapshot);
    }
}