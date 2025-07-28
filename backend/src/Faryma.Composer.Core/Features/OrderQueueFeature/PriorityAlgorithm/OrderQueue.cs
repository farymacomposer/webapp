using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class OrderQueue(List<ReviewOrder> orders)
    {
        private string? _lastIssuedNickname;

        public bool HasOrders => orders.Count > 0;

        /// <summary>
        /// В очереди существует заказ с другим никнеймом
        /// </summary>
        public bool HasOrderFromOtherNickname(string? nicknameToSkip) =>
            orders.Any(x => x.NormalizedNickname != nicknameToSkip);

        /// <summary>
        /// В очереди существует заказ с другим никнеймом и никнейм не совпадает с последним выданным из очереди
        /// </summary>
        public bool HasOrderFromNewNickname(string? nicknameToSkip) =>
            orders.Any(x => x.NormalizedNickname != nicknameToSkip && x.NormalizedNickname != _lastIssuedNickname);

        public ReviewOrder Dequeue(string? nicknameToSkip)
        {
            ReviewOrder? bestMatch = null;
            ReviewOrder? fallback = null;

            foreach (ReviewOrder order in orders)
            {
                if (order.NormalizedNickname != nicknameToSkip)
                {
                    if (order.NormalizedNickname != _lastIssuedNickname)
                    {
                        bestMatch = order;
                        break;
                    }

                    fallback ??= order;
                }
            }

            ReviewOrder result = bestMatch ?? fallback ?? orders[0];
            orders.Remove(result);

            _lastIssuedNickname = result.NormalizedNickname;

            return result;
        }

        public void UpdateOrdersCategory(OrderQueueManager queueManager, OrderCategoryType orderCategoryType, int debtNumber = 0)
        {
            foreach (ReviewOrder item in orders)
            {
                queueManager.OrderPositionsById[item.Id].Current.Category = new OrderCategory
                {
                    Type = orderCategoryType,
                    DebtNumber = debtNumber
                };
            }
        }
    }
}