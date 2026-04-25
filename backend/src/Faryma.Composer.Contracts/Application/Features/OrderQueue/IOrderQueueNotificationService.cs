using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Contracts.Application.Features.OrderQueue
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyQueueUpdated(OrderQueueSnapshot snapshot);
    }
}
