using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class OrderProvider(List<ReviewOrder> ordersQueue)
    {
        private string? _lastIssuedNickname;

        public bool HasOrders => ordersQueue.Count > 0;

        public bool HasAnotherNicknameComparedWithLastIssued(string? nicknameToSkip) =>
            ordersQueue.Any(x => x.UserNickname.NormalizedNickname != nicknameToSkip && x.UserNickname.NormalizedNickname != _lastIssuedNickname);

        public bool HasAnotherNickname(string? nicknameToSkip) =>
            ordersQueue.Any(x => x.UserNickname.NormalizedNickname != nicknameToSkip);

        public ReviewOrder TakeNextOrder(string? nicknameToSkip)
        {
            ReviewOrder? bestMatch = null;
            ReviewOrder? fallback = null;

            foreach (ReviewOrder order in ordersQueue)
            {
                if (order.UserNickname.NormalizedNickname != nicknameToSkip)
                {
                    if (order.UserNickname.NormalizedNickname != _lastIssuedNickname)
                    {
                        bestMatch = order;
                        break;
                    }

                    fallback ??= order;
                }
            }

            ReviewOrder result = bestMatch ?? fallback ?? ordersQueue[0];
            _lastIssuedNickname = result.UserNickname.NormalizedNickname;
            ordersQueue.Remove(result);

            return result;
        }
    }
}