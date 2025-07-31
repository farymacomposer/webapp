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
            PriorityCategory = 2,
            DonationCategory = 3,
            DebtCategories = 4,
            Completed = 5,
        }

        private readonly OrderQueueManager _queueManager;
        private readonly OrderCategory _priorityCategory;
        private readonly OrderCategory? _donationCategory;
        private readonly DebtOrderCategories _debtCategories;
        private State _currentState;
        private string? _lastIssuedNickname;

        public OrderPriorityManager(OrderQueueManager queueManager)
        {
            _queueManager = queueManager;
            _currentState = queueManager.LastOrderPriorityManagerState;

            _priorityCategory = new OrderCategory(queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen && x.Type == ReviewOrderType.OutOfQueue)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            List<(DateOnly, OrderCategory)> queues = queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.ComposerStream.EventDate <= queueManager.CurrentStreamDate)
                .GroupBy(x => x.ComposerStream.EventDate)
                .Select(x => (x.Key, new OrderCategory(x.Order(new OrderPriorityComparer()).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (queues.Count > 0)
            {
                (DateOnly StreamDate, OrderCategory Provider) item = queues.Last();
                if (item.StreamDate == queueManager.CurrentStreamDate)
                {
                    queues.Remove(item);
                    _donationCategory = item.Provider;
                }
            }

            _debtCategories = new DebtOrderCategories(queues);
        }

        public void UpdateOrdersCategories()
        {
            _priorityCategory.UpdateOrdersCategory(_queueManager, OrderCategoryType.OutOfQueue);
            _donationCategory?.UpdateOrdersCategory(_queueManager, OrderCategoryType.Donation);
            _debtCategories.UpdateOrdersCategory(_queueManager);
        }

        public (State NextState, bool IsOnlyNicknameLeft) DetermineNextState()
        {
            (_currentState, bool isOnlyNicknameLeft) = _currentState switch
            {
                State.Initial when _priorityCategory.HasOrders => (State.PriorityCategory, true),
                State.Initial when _donationCategory?.HasOrders == true => (State.DonationCategory, true),
                State.Initial when _debtCategories.HasOrders => (State.DebtCategories, true),

                State.PriorityCategory when _priorityCategory.HasOrderFromNewNickname(_lastIssuedNickname) => (State.PriorityCategory, false),
                State.PriorityCategory when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),
                State.PriorityCategory when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),

                State.DonationCategory when _priorityCategory.HasOrders => (State.PriorityCategory, true),
                State.DonationCategory when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),
                State.DonationCategory when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),

                State.DebtCategories when _priorityCategory.HasOrders => (State.PriorityCategory, true),
                State.DebtCategories when _donationCategory?.HasOrderFromOtherNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),
                State.DebtCategories when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),

                not State.Completed when _priorityCategory.HasOrders => (State.PriorityCategory, true),
                not State.Completed when _donationCategory?.HasOrders == true => (State.DonationCategory, true),
                not State.Completed when _debtCategories.HasOrders => (State.DebtCategories, true),

                _ => (State.Completed, false),
            };

            return (_currentState, isOnlyNicknameLeft);
        }

        public ReviewOrder TakeNextOrder(bool isOnlyNicknameLeft)
        {
            ReviewOrder result = _currentState switch
            {
                State.PriorityCategory => _priorityCategory.Dequeue(_lastIssuedNickname),
                State.DonationCategory => _donationCategory!.Dequeue(_lastIssuedNickname),
                State.DebtCategories when isOnlyNicknameLeft => _debtCategories.DequeueRoundRobin(_lastIssuedNickname),
                State.DebtCategories when isOnlyNicknameLeft == false => _debtCategories.DequeueRoundRobinFromOtherNickname(_lastIssuedNickname),
            };

            _lastIssuedNickname = result.MainNormalizedNickname;

            return result;
        }
    }
}