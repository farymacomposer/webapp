using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Testing.Infrastructure
{
    /// <summary>
    /// Сохраняет уведомления очереди, чтобы тесты могли проверить факт обновления.
    /// </summary>
    public sealed class TestOrderQueueNotificationService : IOrderQueueNotificationService
    {
        private readonly Lock _sync = new();
        private readonly List<OrderQueueSnapshot> _snapshots = [];
        private TaskCompletionSource _nextUpdate = CreateWaitSource();

        /// <summary>
        /// Возвращает количество полученных обновлений очереди.
        /// </summary>
        public int UpdateCount
        {
            get
            {
                lock (_sync)
                {
                    return _snapshots.Count;
                }
            }
        }

        /// <summary>
        /// Возвращает полученные снимки очереди в порядке поступления.
        /// </summary>
        public IReadOnlyList<OrderQueueSnapshot> Snapshots
        {
            get
            {
                lock (_sync)
                {
                    return [.. _snapshots];
                }
            }
        }

        /// <summary>
        /// Фиксирует новое состояние очереди для последующей проверки в тесте.
        /// </summary>
        public Task NotifyQueueUpdated(OrderQueueSnapshot snapshot)
        {
            TaskCompletionSource completedSource;

            lock (_sync)
            {
                _snapshots.Add(snapshot);
                completedSource = _nextUpdate;
                _nextUpdate = CreateWaitSource();
            }

            completedSource.TrySetResult();

            return Task.CompletedTask;
        }

        private static TaskCompletionSource CreateWaitSource() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
