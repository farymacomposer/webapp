using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public interface IOrderQueueNotificationService
    {
        Task NotifyQueueUpdated(OrderQueueSnapshot snapshot);
    }
}
