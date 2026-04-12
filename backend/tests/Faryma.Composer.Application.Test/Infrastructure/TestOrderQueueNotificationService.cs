using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Application.Test.Infrastructure
{
    public sealed class TestOrderQueueNotificationService : IOrderQueueNotificationService
    {
        private readonly object _sync = new();
        private readonly List<OrderQueueSnapshot> _snapshots = [];
        private TaskCompletionSource _nextUpdate = CreateWaitSource();

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

        public async Task WaitForCountAsync(int expectedCount, TimeSpan timeout)
        {
            using CancellationTokenSource cts = new(timeout);

            while (true)
            {
                Task waitTask;

                lock (_sync)
                {
                    if (_snapshots.Count >= expectedCount)
                    {
                        return;
                    }

                    waitTask = _nextUpdate.Task;
                }

                await waitTask.WaitAsync(cts.Token);
            }
        }

        private static TaskCompletionSource CreateWaitSource() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}