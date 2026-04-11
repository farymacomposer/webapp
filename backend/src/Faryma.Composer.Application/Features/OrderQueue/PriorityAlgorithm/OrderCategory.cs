using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm
{
    /// <summary>
    /// Категория заказов
    /// </summary>
    public sealed class OrderCategory(List<ReviewOrderEntity> orders)
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
        /// Устанавливает последний выданный никнейм
        /// </summary>
        public void SetLastIssuedNickname(string? nickname) => _lastIssuedNickname = nickname;

        /// <summary>
        /// В категории существует заказ с другим никнеймом
        /// </summary>
        public bool HasOrderSkippingNickname(string? nicknameToSkip) =>
            orders.Any(x => x.MainNormalizedNickname != nicknameToSkip);

        /// <summary>
        /// В категории существует заказ с другим никнеймом и никнейм не совпадает с последним выданным никнеймом из данной категории
        /// </summary>
        public bool HasOrderSkippingNicknameAndCategoryLast(string? nicknameToSkip) =>
            orders.Any(x => x.MainNormalizedNickname != nicknameToSkip && x.MainNormalizedNickname != _lastIssuedNickname);

        /// <summary>
        /// Извлекает заказ из категории
        /// </summary>
        public ReviewOrderEntity Dequeue(string? nicknameToSkip)
        {
            ReviewOrderEntity? bestMatch = null;
            ReviewOrderEntity? fallback = null;
            ReviewOrderEntity first = orders[0];

            // bestMatch - заказ с никнеймом, отличным и от глобально последнего выданного никнейма, и от последнего никнейма, выданного внутри этой категории
            // fallback - заказ с никнеймом, отличным от глобально последнего выданного никнейма
            // first - первый заказ в категории, если альтернатив по никнейму уже нет
            foreach (ReviewOrderEntity order in orders)
            {
                if (order.MainNormalizedNickname != nicknameToSkip)
                {
                    if (order.MainNormalizedNickname != _lastIssuedNickname)
                    {
                        bestMatch = order;
                        break;
                    }

                    fallback ??= order;
                }
            }

            ReviewOrderEntity result = bestMatch ?? fallback ?? first;
            orders.Remove(result);

            _lastIssuedNickname = result.MainNormalizedNickname;

            return result;
        }

        /// <summary>
        /// Обновляет категорию заказов
        /// </summary>
        public void UpdateOrdersCategory(OrderQueueManager queueManager, QueueCategory queueCategory, int debtIndex = 0)
        {
            foreach (ReviewOrderEntity item in orders)
            {
                queueManager.OrderPositionsById[item.Id].UpdateCurrentCategory(queueCategory, debtIndex);
            }
        }
    }
}