using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    /// <summary>
    /// Долговые категории
    /// </summary>
    public sealed class DebtOrderCategories(List<(DateOnly StreamDate, OrderCategory Category)> debtCategoriesByStreamDate)
    {
        /// <summary>
        /// Счетчик для чередования долговых категорий (X3-X2-X1)
        /// </summary>
        private int _roundRobinCounter;

        /// <summary>
        /// В категориях есть заказы
        /// </summary>
        public bool HasOrders => debtCategoriesByStreamDate.Any(x => x.Category.HasOrders);

        /// <summary>
        /// В категориях существует заказ с другим никнеймом и никнейм не совпадает с последним выданным никнеймом из категорий
        /// </summary>
        public bool HasOrderFromNewNickname(string? nicknameToSkip) => debtCategoriesByStreamDate.Any(x => x.Category.HasOrderFromNewNickname(nicknameToSkip));

        /// <summary>
        /// Последовательно перебирает долговые категории (X3-X2-X1) и извлекает заказ из категории, в которой есть заказы
        /// </summary>
        public ReviewOrder DequeueRoundRobin(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % debtCategoriesByStreamDate.Count;

                (DateOnly streamDate, OrderCategory category) = debtCategoriesByStreamDate[index];
                if (category.HasOrders)
                {
                    ReviewOrder order = category.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }

        /// <summary>
        /// Последовательно перебирает долговые категории (X3-X2-X1) и извлекает заказ из категории, в которой существует заказ с другим никнеймом
        /// </summary>
        public ReviewOrder DequeueRoundRobinFromOtherNickname(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % debtCategoriesByStreamDate.Count;

                (DateOnly streamDate, OrderCategory category) = debtCategoriesByStreamDate[index];
                if (category.HasOrderFromOtherNickname(nicknameToSkip))
                {
                    ReviewOrder order = category.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }

        /// <summary>
        /// Обновляет категорию заказов
        /// </summary>
        public void UpdateOrdersCategory(OrderQueueManager queueManager)
        {
            int debtNumber = 1;
            foreach ((DateOnly streamDate, OrderCategory category) in debtCategoriesByStreamDate.AsEnumerable().Reverse())
            {
                category.UpdateOrdersCategory(queueManager, OrderCategoryType.Debt, debtNumber);
                debtNumber++;
            }
        }
    }
}