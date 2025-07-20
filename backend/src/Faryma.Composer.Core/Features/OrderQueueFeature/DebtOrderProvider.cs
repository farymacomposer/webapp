using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class DebtOrderProvider(List<(DateOnly StreamDate, OrderProvider Provider)> orderProviders)
    {
        private int _counter;

        public bool HasOrders => orderProviders.Any(x => x.Provider.HasOrders);

        public bool HasAnotherNickname(string? lastIssuedNickname) => orderProviders.Any(x => x.Provider.HasAnotherNickname(lastIssuedNickname));

        public ReviewOrder TakeNextOrder(string? nicknameToSkip, bool isOnlyNicknameLeft)
        {
            while (true)
            {
                int index = _counter % orderProviders.Count;

                (DateOnly streamDate, OrderProvider provider) = orderProviders[index];
                if (isOnlyNicknameLeft)
                {
                    if (provider.HasOrders)
                    {
                        ReviewOrder order = provider.TakeNextOrder(nicknameToSkip);
                        _counter++;

                        return order;
                    }
                }
                else
                {
                    if (provider.HasAnotherNickname(nicknameToSkip))
                    {
                        ReviewOrder order = provider.TakeNextOrder(nicknameToSkip);
                        _counter++;

                        return order;
                    }
                }

                _counter++;
            }
        }
    }
}