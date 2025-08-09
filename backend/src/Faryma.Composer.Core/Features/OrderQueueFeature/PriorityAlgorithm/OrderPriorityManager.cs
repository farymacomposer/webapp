using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    /// <summary>
    /// Менеджер приоритетов активных заказов, определяющий порядок обработки
    /// </summary>
    public sealed class OrderPriorityManager
    {
        /// <summary>
        /// Категория заказов вне очереди
        /// </summary>
        private readonly OrderCategory _outOfQueueCategory;

        /// <summary>
        /// Донатная категория заказов (может отсутствовать)
        /// </summary>
        private readonly OrderCategory? _donationCategory;

        /// <summary>
        /// Коллекция долговых категорий заказов
        /// </summary>
        private readonly DebtOrderCategories _debtCategories;

        /// <summary>
        /// Текущее состояние менеджера приоритетов
        /// </summary>
        private CategoryState _currentState;

        /// <summary>
        /// Последний обработанный никнейм (для предотвращения повторной обработки одного пользователя подряд)
        /// </summary>
        private string? _lastIssuedNickname;

        public OrderPriorityManager(OrderQueueManager queueManager)
        {
            _currentState = queueManager.LastPriorityManagerState;
            _lastIssuedNickname = queueManager.LastIssuedNickname;

            _outOfQueueCategory = new OrderCategory(queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type == ReviewOrderType.OutOfQueue
                    && x.Status is ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            _outOfQueueCategory.SetLastIssuedNickname(queueManager.LastOutOfQueueNickname);
            _outOfQueueCategory.UpdateOrdersCategory(queueManager, OrderCategoryType.OutOfQueue);

            List<(DateOnly StreamDate, OrderCategory Category)> categories = queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.Status is ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending
                    && x.CreationStream.EventDate <= queueManager.CurrentStreamDate)
                .GroupBy(x => x.CreationStream.EventDate)
                .Select(x => (x.Key, new OrderCategory(x.Order(OrderPriorityComparer.Default).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (categories.Count > 0)
            {
                foreach ((DateOnly streamDate, OrderCategory category) in categories)
                {
                    if (queueManager.LastNicknameByStreamDate.TryGetValue(streamDate, out string? nickname))
                    {
                        category.SetLastIssuedNickname(nickname);
                    }
                }

                (DateOnly StreamDate, OrderCategory Category) item = categories.Last();
                if (item.StreamDate == queueManager.CurrentStreamDate)
                {
                    categories.Remove(item);
                    _donationCategory = item.Category;
                    _donationCategory.UpdateOrdersCategory(queueManager, OrderCategoryType.Donation);
                }
            }

            _debtCategories = new DebtOrderCategories(categories);
            _debtCategories.UpdateOrderCategories(queueManager);
        }

        /// <summary>
        /// Определяет следующее состояние обработки заказов на основе текущего состояния и доступных категорий
        /// </summary>
        /// <returns>
        /// Кортеж, содержащий следующее состояние и флаг, указывающий, остался ли только один никнейм для обработки
        /// </returns>
        public (CategoryState NextState, bool IsOnlyNicknameLeft) DetermineNextState()
        {
            (_currentState, bool isOnlyNicknameLeft) = _currentState switch
            {
                CategoryState.Initial when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                CategoryState.Initial when _donationCategory?.HasOrders == true => (CategoryState.Donation, true),
                CategoryState.Initial when _debtCategories.HasOrders => (CategoryState.Debt, true),

                CategoryState.OutOfQueue when _outOfQueueCategory.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.OutOfQueue, false),
                CategoryState.OutOfQueue when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (CategoryState.Donation, false),
                CategoryState.OutOfQueue when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.Debt, false),

                CategoryState.Donation when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                CategoryState.Donation when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.Debt, false),
                CategoryState.Donation when _donationCategory?.HasOrderFromNewNickname(_lastIssuedNickname) == true => (CategoryState.Donation, false),

                CategoryState.Debt when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                CategoryState.Debt when _donationCategory?.HasOrderFromOtherNickname(_lastIssuedNickname) == true => (CategoryState.Donation, false),
                CategoryState.Debt when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.Debt, false),

                not CategoryState.Completed when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                not CategoryState.Completed when _donationCategory?.HasOrders == true => (CategoryState.Donation, true),
                not CategoryState.Completed when _debtCategories.HasOrders => (CategoryState.Debt, true),

                _ => (CategoryState.Completed, false),
            };

            return (_currentState, isOnlyNicknameLeft);
        }

        /// <summary>
        /// Извлекает следующий заказ для обработки в соответствии с текущим состоянием
        /// </summary>
        /// <param name="isOnlyNicknameLeft">Флаг, указывающий, остался ли только один никнейм для обработки</param>
        /// <returns>Следующий заказ для обработки</returns>
        public ReviewOrder TakeNextOrder(bool isOnlyNicknameLeft)
        {
            ReviewOrder result = _currentState switch
            {
                CategoryState.OutOfQueue => _outOfQueueCategory.Dequeue(_lastIssuedNickname),
                CategoryState.Donation => _donationCategory!.Dequeue(_lastIssuedNickname),
                CategoryState.Debt when isOnlyNicknameLeft => _debtCategories.DequeueRoundRobin(_lastIssuedNickname),
                CategoryState.Debt when isOnlyNicknameLeft == false => _debtCategories.DequeueRoundRobinFromOtherNickname(_lastIssuedNickname),
                _ => throw new OrderQueueException($"Тип категории очереди '{_currentState}' не поддерживается")
            };

            _lastIssuedNickname = result.MainNormalizedNickname;

            return result;
        }
    }
}