using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm
{
    /// <summary>
    /// Долговые категории
    /// </summary>
    public sealed class DebtOrderCategories
    {
        /// <summary>
        /// Долговые категории по дате стрима
        /// </summary>
        private readonly List<(DateOnly StreamDate, OrderCategory Category)> _debtCategoriesByStreamDate;

        /// <summary>
        /// Счетчик для чередования долговых категорий
        /// </summary>
        private int _roundRobinCounter;

        /// <summary>
        /// В категориях есть заказы
        /// </summary>
        public bool HasOrders => _debtCategoriesByStreamDate.Any(x => x.Category.HasOrders);

        public DebtOrderCategories(OrderQueueManager queueManager, List<(DateOnly StreamDate, OrderCategory Category)> debtCategoriesByStreamDate)
        {
            _debtCategoriesByStreamDate = debtCategoriesByStreamDate;

            if (debtCategoriesByStreamDate.Count > 0)
            {
                DateOnly? lastDebtCategoryDate = queueManager.PriorityManagerState.LastDebtCategoryDate;

                int debtIndex = 0;
                foreach ((DateOnly streamDate, OrderCategory category) in debtCategoriesByStreamDate.AsEnumerable().Reverse())
                {
                    if (streamDate > lastDebtCategoryDate)
                    {
                        _roundRobinCounter = debtCategoriesByStreamDate.Count - debtIndex - 1;
                    }

                    category.UpdateOrdersCategory(queueManager, QueueCategory.Debt, debtIndex);
                    debtIndex++;
                }
            }
        }

        /// <summary>
        /// В категориях существует заказ с другим никнеймом и никнейм не совпадает с последним выданным никнеймом из категорий
        /// </summary>
        public bool HasOrderFromNewNickname(string? nicknameToSkip) => _debtCategoriesByStreamDate.Any(x => x.Category.HasOrderSkippingNicknameAndCategoryLast(nicknameToSkip));

        /// <summary>
        /// Последовательно перебирает долговые категории и извлекает заказ из категории, в которой есть заказы
        /// </summary>
        public ReviewOrderEntity DequeueRoundRobin(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % _debtCategoriesByStreamDate.Count;

                (DateOnly streamDate, OrderCategory category) = _debtCategoriesByStreamDate[index];
                if (category.HasOrders)
                {
                    ReviewOrderEntity order = category.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }

        /// <summary>
        /// Последовательно перебирает долговые категории и извлекает заказ из категории, в которой существует заказ с другим никнеймом
        /// </summary>
        public ReviewOrderEntity DequeueRoundRobinFromOtherNickname(string? nicknameToSkip)
        {
            while (true)
            {
                int index = _roundRobinCounter % _debtCategoriesByStreamDate.Count;

                (DateOnly streamDate, OrderCategory category) = _debtCategoriesByStreamDate[index];
                if (category.HasOrderSkippingNickname(nicknameToSkip))
                {
                    ReviewOrderEntity order = category.Dequeue(nicknameToSkip);
                    _roundRobinCounter++;

                    return order;
                }

                _roundRobinCounter++;
            }
        }
    }
}
