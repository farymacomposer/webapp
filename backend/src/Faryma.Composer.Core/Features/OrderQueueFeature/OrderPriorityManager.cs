using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderPriorityManager
    {
        public enum State
        {
            Initial = 0,
            OutOfQueue = 1,
            Donation = 2,
            Debt = 3,
            Completed = 4,
        }

        private readonly OrderProvider _outOfQueueProvider;
        private readonly OrderProvider? _donationProvider;
        private readonly DebtOrderProvider _debtProvider;
        private State _currentState = State.Initial;
        private string? _lastIssuedNickname;

        public OrderPriorityManager(DateOnly currentStreamDate, Dictionary<long, ReviewOrder> orders)
        {
            _outOfQueueProvider = new OrderProvider(orders
                .Select(x => x.Value)
                .Where(x => x.IsActive && x.Type == ReviewOrderType.OutOfQueue)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            ReviewOrderComparer comparer = new();
            List<(DateOnly, OrderProvider)> providers = orders
                .Select(x => x.Value)
                .Where(x => x.IsActive
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.ComposerStream.EventDate <= currentStreamDate)
                .GroupBy(x => x.ComposerStream.EventDate)
                .Select(x => (x.Key, new OrderProvider(x.Order(comparer).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (providers.Count > 0)
            {
                (DateOnly StreamDate, OrderProvider Provider) item = providers.Last();
                if (item.StreamDate == currentStreamDate)
                {
                    providers.Remove(item);
                    _donationProvider = item.Provider;
                }
            }

            _debtProvider = new DebtOrderProvider(providers);
        }

        public (State, bool) DetermineNextState()
        {
            (_currentState, bool isOnlyNicknameLeft) = _currentState switch
            {
                State.Initial or State.Debt when _outOfQueueProvider.HasAnotherNickname(_lastIssuedNickname) => (State.OutOfQueue, false),
                State.Initial or State.Debt when _donationProvider?.HasAnotherNickname(_lastIssuedNickname) == true => (State.Donation, false),
                State.Initial or State.Debt when _debtProvider.HasAnotherNickname(_lastIssuedNickname) => (State.Debt, false),

                State.OutOfQueue when _outOfQueueProvider.HasAnotherNickname(_lastIssuedNickname) => (State.OutOfQueue, false),
                State.OutOfQueue when _donationProvider?.HasAnotherNickname(_lastIssuedNickname) == true => (State.Donation, false),
                State.OutOfQueue when _debtProvider.HasAnotherNickname(_lastIssuedNickname) => (State.Debt, false),

                State.Donation when _outOfQueueProvider.HasAnotherNickname(_lastIssuedNickname) => (State.OutOfQueue, false),
                State.Donation when _debtProvider.HasAnotherNickname(_lastIssuedNickname) => (State.Debt, false),
                State.Donation when _donationProvider?.HasAnotherNickname(_lastIssuedNickname) == true => (State.Donation, false),

                State.Initial or State.Debt or State.OutOfQueue or State.Donation when _outOfQueueProvider.HasOrders => (State.OutOfQueue, true),
                State.Initial or State.Debt or State.OutOfQueue or State.Donation when _donationProvider?.HasOrders == true => (State.Donation, true),
                State.Initial or State.Debt or State.OutOfQueue or State.Donation when _debtProvider.HasOrders => (State.Debt, true),

                _ => (State.Completed, false),
            };

            return (_currentState, isOnlyNicknameLeft);
        }

        public ReviewOrder TakeNextOrder(bool isOnlyNicknameLeft)
        {
            ReviewOrder order = _currentState switch
            {
                State.OutOfQueue => _outOfQueueProvider.TakeNextOrder(_lastIssuedNickname),
                State.Donation => _donationProvider!.TakeNextOrder(_lastIssuedNickname),
                State.Debt => _debtProvider.TakeNextOrder(_lastIssuedNickname, isOnlyNicknameLeft),
            };

            _lastIssuedNickname = order.UserNickname.NormalizedNickname;

            return order;
        }
    }
}