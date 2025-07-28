using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class OrderPriorityManager
    {
        public enum State
        {
            Unspecified = 0,
            Initial = 1,
            PriorityQueue = 2,
            DonationQueue = 3,
            DebtQueues = 4,
            Completed = 5,
        }

        private readonly OrderQueueManager _queueManager;
        private readonly OrderQueue _priorityQueue;
        private readonly OrderQueue? _donationQueue;
        private readonly DebtOrderQueues _debtQueues;
        private State _currentState;
        private string? _lastIssuedNickname;

        public OrderPriorityManager(OrderQueueManager queueManager)
        {
            _queueManager = queueManager;
            _currentState = queueManager.LastOrderPriorityManagerState;

            _priorityQueue = new OrderQueue(queueManager.OrdersById
                .Select(x => x.Value)
                .Where(x => !x.IsFrozen && x.Type == ReviewOrderType.OutOfQueue)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            OrderPriorityComparer comparer = new();
            List<(DateOnly, OrderQueue)> queues = queueManager.OrdersById
                .Select(x => x.Value)
                .Where(x => !x.IsFrozen
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.ComposerStream.EventDate <= queueManager.CurrentStreamDate)
                .GroupBy(x => x.ComposerStream.EventDate)
                .Select(x => (x.Key, new OrderQueue(x.Order(comparer).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (queues.Count > 0)
            {
                (DateOnly StreamDate, OrderQueue Provider) item = queues.Last();
                if (item.StreamDate == queueManager.CurrentStreamDate)
                {
                    queues.Remove(item);
                    _donationQueue = item.Provider;
                }
            }

            _debtQueues = new DebtOrderQueues(queues);
        }

        public void UpdateOrdersCategories()
        {
            _priorityQueue.UpdateOrdersCategory(_queueManager, OrderCategoryType.OutOfQueue);
            _donationQueue?.UpdateOrdersCategory(_queueManager, OrderCategoryType.Donation);
            _debtQueues.UpdateOrdersCategory(_queueManager);
        }

        public (State NextState, bool IsOnlyNicknameLeft) DetermineNextState()
        {
            (_currentState, bool isOnlyNicknameLeft) = _currentState switch
            {
                State.Initial when _priorityQueue.HasOrders => (State.PriorityQueue, true),
                State.Initial when _donationQueue?.HasOrders == true => (State.DonationQueue, true),
                State.Initial when _debtQueues.HasOrders => (State.DebtQueues, true),

                State.PriorityQueue when _priorityQueue.HasOrderFromNewNickname(_lastIssuedNickname) => (State.PriorityQueue, false),
                State.PriorityQueue when _donationQueue?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationQueue, false),
                State.PriorityQueue when _debtQueues.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtQueues, false),

                State.DonationQueue when _priorityQueue.HasOrders => (State.PriorityQueue, true),
                State.DonationQueue when _debtQueues.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtQueues, false),
                State.DonationQueue when _donationQueue?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationQueue, false),

                State.DebtQueues when _priorityQueue.HasOrders => (State.PriorityQueue, true),
                State.DebtQueues when _donationQueue?.HasOrderFromOtherNickname(_lastIssuedNickname) == true => (State.DonationQueue, false),
                State.DebtQueues when _debtQueues.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtQueues, false),

                not State.Completed when _priorityQueue.HasOrders => (State.PriorityQueue, true),
                not State.Completed when _donationQueue?.HasOrders == true => (State.DonationQueue, true),
                not State.Completed when _debtQueues.HasOrders => (State.DebtQueues, true),

                _ => (State.Completed, false),
            };

            return (_currentState, isOnlyNicknameLeft);
        }

        public ReviewOrder TakeNextOrder(bool isOnlyNicknameLeft)
        {
            ReviewOrder result = _currentState switch
            {
                State.PriorityQueue => _priorityQueue.Dequeue(_lastIssuedNickname),
                State.DonationQueue => _donationQueue!.Dequeue(_lastIssuedNickname),
                State.DebtQueues when isOnlyNicknameLeft => _debtQueues.DequeueRoundRobin(_lastIssuedNickname),
                State.DebtQueues when isOnlyNicknameLeft == false => _debtQueues.DequeueRoundRobinFromOtherNickname(_lastIssuedNickname),
            };

            _lastIssuedNickname = result.NormalizedNickname;

            return result;
        }
    }
}