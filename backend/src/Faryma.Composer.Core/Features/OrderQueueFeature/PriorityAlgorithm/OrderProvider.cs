using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class OrderProvider(List<ReviewOrder> ordersQueue)
    {
        private string? _lastIssuedNickname;

        public bool HasOrders => ordersQueue.Count > 0;

        public bool HasAnotherNickname(string? nicknameToSkip) => ordersQueue.Any(x =>
            x.UserNickname.NormalizedNickname != _lastIssuedNickname
                && x.UserNickname.NormalizedNickname != nicknameToSkip);

        public ReviewOrder TakeNextOrder(string? nicknameToSkip)
        {
            ReviewOrder? bestMatch = null;

            foreach (ReviewOrder order in ordersQueue)
            {
                if (order.UserNickname.NormalizedNickname != nicknameToSkip)
                {
                    bestMatch = order;
                    break;
                }
            }

            ReviewOrder result = bestMatch ?? ordersQueue[0];
            _lastIssuedNickname = result.UserNickname.NormalizedNickname;
            ordersQueue.Remove(result);

            return result;
        }
    }
}