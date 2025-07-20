using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class DebtOrderProvider(List<(DateOnly StreamDate, OrderProvider Provider)> orderProviders)
    {
        private int _counter;

        public bool HasOrders => orderProviders.Any(x => x.Provider.HasOrders);

        public ReviewOrder TakeNextOrder(string? nicknameToSkip)
        {
            int index = _counter % orderProviders.Count;

            while (orderProviders.Count > 0)
            {
                if (index > orderProviders.Count - 1)
                {
                    index--;
                }

                (DateOnly StreamDate, OrderProvider Provider) item = orderProviders[index];
                if (item.Provider.HasOrders)
                {
                    ReviewOrder order = item.Provider.TakeNextOrder(nicknameToSkip);
                    _counter++;

                    return order;
                }
                else
                {
                    orderProviders.Remove(item);
                }
            }

            return null!;
        }
    }
}