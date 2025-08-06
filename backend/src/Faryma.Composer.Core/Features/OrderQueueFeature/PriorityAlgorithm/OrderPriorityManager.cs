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

            /// <summary>
            /// Категория - вне очереди
            /// </summary>
            OutOfQueueCategory = 2,

            /// <summary>
            /// Донатная категория
            /// </summary>
            DonationCategory = 3,

            /// <summary>
            /// Долговые категории
            /// </summary>
            DebtCategories = 4,
            Completed = 5,
        }

        private readonly OrderQueueManager _queueManager;
        private readonly OrderCategory _outOfQueueCategory;
        private readonly OrderCategory? _donationCategory;
        private readonly DebtOrderCategories _debtCategories;
        private State _currentState;
        private string? _lastIssuedNickname;

        public OrderPriorityManager(OrderQueueManager queueManager)
        {
            _queueManager = queueManager;
            _currentState = queueManager.LastOrderPriorityManagerState;

            _outOfQueueCategory = new OrderCategory(queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen && x.Type == ReviewOrderType.OutOfQueue)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            List<(DateOnly, OrderCategory)> categories = queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.CreationStream.EventDate <= queueManager.CurrentStreamDate)
                .GroupBy(x => x.CreationStream.EventDate)
                .Select(x => (x.Key, new OrderCategory(x.Order(new OrderPriorityComparer()).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (categories.Count > 0)
            {
                (DateOnly StreamDate, OrderCategory Provider) item = categories.Last();
                if (item.StreamDate == queueManager.CurrentStreamDate)
                {
                    categories.Remove(item);
                    _donationCategory = item.Provider;
                }
            }

            _debtCategories = new DebtOrderCategories(categories);
        }

        public void UpdateOrdersCategories()
        {
            _outOfQueueCategory.UpdateOrdersCategory(_queueManager, OrderCategoryType.OutOfQueue);
            _donationCategory?.UpdateOrdersCategory(_queueManager, OrderCategoryType.Donation);
            _debtCategories.UpdateOrdersCategory(_queueManager);
        }

        public (State NextState, bool IsOnlyNicknameLeft) DetermineNextState()
        {
            (_currentState, bool isOnlyNicknameLeft) = _currentState switch
            {
                State.Initial when _outOfQueueCategory.HasOrders => (State.OutOfQueueCategory, true),
                State.Initial when _donationCategory?.HasOrders == true => (State.DonationCategory, true),
                State.Initial when _debtCategories.HasOrders => (State.DebtCategories, true),

                State.OutOfQueueCategory when _outOfQueueCategory.HasOrderFromNewNickname(_lastIssuedNickname) => (State.OutOfQueueCategory, false),
                State.OutOfQueueCategory when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),
                State.OutOfQueueCategory when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),

                State.DonationCategory when _outOfQueueCategory.HasOrders => (State.OutOfQueueCategory, true),
                State.DonationCategory when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),
                State.DonationCategory when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),

                State.DebtCategories when _outOfQueueCategory.HasOrders => (State.OutOfQueueCategory, true),
                State.DebtCategories when _donationCategory?.HasOrderFromOtherNickname(_lastIssuedNickname) == true => (State.DonationCategory, false),
                State.DebtCategories when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (State.DebtCategories, false),

                not State.Completed when _outOfQueueCategory.HasOrders => (State.OutOfQueueCategory, true),
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
                State.OutOfQueueCategory => _outOfQueueCategory.Dequeue(_lastIssuedNickname),
                State.DonationCategory => _donationCategory!.Dequeue(_lastIssuedNickname),
                State.DebtCategories when isOnlyNicknameLeft => _debtCategories.DequeueRoundRobin(_lastIssuedNickname),
                State.DebtCategories when isOnlyNicknameLeft == false => _debtCategories.DequeueRoundRobinFromOtherNickname(_lastIssuedNickname),
            };

            _lastIssuedNickname = result.MainNormalizedNickname;

            return result;
        }
    }
}