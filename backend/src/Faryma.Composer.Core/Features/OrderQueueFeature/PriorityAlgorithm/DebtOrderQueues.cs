using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class DebtOrderQueues(List<(DateOnly StreamDate, OrderQueue Queue)> queuesByStreamDate)
    {
        private int _roundRobinCounter;

        public bool HasOrders => queuesByStreamDate.Any(x => x.Queue.HasOrders);

        public bool HasOrderFromNewNickname(string? nicknameToSkip) => queuesByStreamDate.Any(x => x.Queue.HasOrderFromNewNickname(nicknameToSkip));

        public ReviewOrder DequeueRoundRobin(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % queuesByStreamDate.Count;

                (DateOnly streamDate, OrderQueue queue) = queuesByStreamDate[index];
                if (queue.HasOrders)
                {
                    ReviewOrder order = queue.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }

        public ReviewOrder DequeueRoundRobinFromOtherNickname(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % queuesByStreamDate.Count;

                (DateOnly streamDate, OrderQueue queue) = queuesByStreamDate[index];
                if (queue.HasOrderFromOtherNickname(nicknameToSkip))
                {
                    ReviewOrder order = queue.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }

        public void UpdateOrdersCategory(OrderQueueManager queueManager)
        {
            int debtNumber = 1;
            foreach ((DateOnly streamDate, OrderQueue queue) in queuesByStreamDate.AsEnumerable().Reverse())
            {
                queue.UpdateOrdersCategory(queueManager, OrderCategoryType.Debt, debtNumber);
                debtNumber++;
            }
        }
    }
}