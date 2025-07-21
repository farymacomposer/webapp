using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class DebtOrderProvider(List<(DateOnly StreamDate, OrderProvider Provider)> orderProviders)
    {
        private int _counter;

        public bool HasOrders => orderProviders.Any(x => x.Provider.HasOrders);

        public bool HasAnotherNicknameComparedWithLastIssued(string? nicknameToSkip) => orderProviders.Any(x => x.Provider.HasAnotherNicknameComparedWithLastIssued(nicknameToSkip));

        public ReviewOrder TakeNextOrderFromAnyProvider(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _counter % orderProviders.Count;

                (DateOnly streamDate, OrderProvider provider) = orderProviders[index];
                if (provider.HasOrders)
                {
                    ReviewOrder order = provider.TakeNextOrder(nicknameToSkip);
                    _counter++;

                    return order;
                }

                _counter++;
            }
        }

        public ReviewOrder TakeNextOrder(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _counter % orderProviders.Count;

                (DateOnly streamDate, OrderProvider provider) = orderProviders[index];
                if (provider.HasAnotherNicknameComparedWithLastIssued(nicknameToSkip))
                {
                    ReviewOrder order = provider.TakeNextOrder(nicknameToSkip);
                    _counter++;

                    return order;
                }

                _counter++;
            }
        }
    }
}