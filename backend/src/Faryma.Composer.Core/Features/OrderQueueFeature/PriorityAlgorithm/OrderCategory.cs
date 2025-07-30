using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    /// <summary>
    /// Категория заказов
    /// <para>в одной категории может быть только один тип заказов, с одного стрима</para>
    /// </summary>
    public sealed class OrderCategory(List<ReviewOrder> orders)
    {
        /// <summary>
        /// Последний выданный никнейм из категории
        /// </summary>
        private string? _lastIssuedNickname;

        /// <summary>
        /// В категории есть заказы
        /// </summary>
        public bool HasOrders => orders.Count > 0;

        /// <summary>
        /// В категории существует заказ с другим никнеймом
        /// </summary>
        public bool HasOrderFromOtherNickname(string? nicknameToSkip) =>
            orders.Any(x => x.NormalizedNickname != nicknameToSkip);

        /// <summary>
        /// В категории существует заказ с другим никнеймом и никнейм не совпадает с последним выданным никнеймом из данной категории
        /// </summary>
        public bool HasOrderFromNewNickname(string? nicknameToSkip) =>
            orders.Any(x => x.NormalizedNickname != nicknameToSkip && x.NormalizedNickname != _lastIssuedNickname);

        /// <summary>
        /// Извлекает заказ из категории
        /// </summary>
        public ReviewOrder Dequeue(string? nicknameToSkip)
        {
            ReviewOrder? bestMatch = null;
            ReviewOrder? fallback = null;
            ReviewOrder first = orders[0];

            // bestMatch - когда в категории есть никнейм, который не совпадает с последним прослушанным `nicknameToSkip` и с последним из данной категории `_lastIssuedNickname`
            // fallback - когда в категории есть никнейм, который не совпадает с последним прослушанным `nicknameToSkip`
            // first - если в категории остались заказы, совпадающие с `nicknameToSkip`
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

            ReviewOrder result = bestMatch ?? fallback ?? first;
            orders.Remove(result);

            _lastIssuedNickname = result.NormalizedNickname;

            return result;
        }

        /// <summary>
        /// Обновляет категорию заказов
        /// </summary>
        public void UpdateOrdersCategory(OrderQueueManager queueManager, OrderCategoryType orderCategoryType, int debtNumber = 0)
        {
            foreach (ReviewOrder item in orders)
            {
                queueManager.OrderPositionsById[item.Id].Current.Category = new OrderCategoryInfo
                {
                    Type = orderCategoryType,
                    DebtNumber = debtNumber
                };
            }
        }
    }
}