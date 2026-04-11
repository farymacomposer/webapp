using System.Diagnostics;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm
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
            OrderPriorityManagerState state = queueManager.PriorityManagerState;

            _currentState = state.LastPriorityManagerState;
            _lastIssuedNickname = state.LastIssuedNickname;

            _outOfQueueCategory = new OrderCategory(queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type == ReviewOrderType.OutOfQueue
                    && x.Status is ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToList());

            _outOfQueueCategory.SetLastIssuedNickname(state.LastOutOfQueueNickname);
            _outOfQueueCategory.UpdateOrdersCategory(queueManager, QueueCategory.OutOfQueue);

            List<(DateOnly StreamDate, OrderCategory Category)> activeOrderCategories = queueManager.OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen
                    && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free
                    && x.Status is ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending
                    && x.CreationStream.EventDate <= queueManager.NearestStreamDate)
                .GroupBy(x => x.CreationStream.EventDate)
                .Select(x => (x.Key, new OrderCategory(x.Order(OrderPriorityComparer.Default).ToList())))
                .OrderBy(x => x.Key)
                .ToList();

            if (activeOrderCategories.Count > 0)
            {
                foreach ((DateOnly streamDate, OrderCategory category) in activeOrderCategories)
                {
                    if (state.LastNicknamesByStreamDate.TryGetValue(streamDate, out string? nickname))
                    {
                        category.SetLastIssuedNickname(nickname);
                    }
                }

                (DateOnly StreamDate, OrderCategory Category) item = activeOrderCategories.Last();
                if (item.StreamDate == queueManager.NearestStreamDate)
                {
                    activeOrderCategories.Remove(item);
                    _donationCategory = item.Category;
                    _donationCategory.UpdateOrdersCategory(queueManager, QueueCategory.Donation);
                }
            }

            _debtCategories = new DebtOrderCategories(queueManager, activeOrderCategories);
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

                CategoryState.OutOfQueue when _outOfQueueCategory.HasOrderSkippingNicknameAndCategoryLast(_lastIssuedNickname) => (CategoryState.OutOfQueue, false),
                CategoryState.OutOfQueue when _donationCategory?.HasOrderSkippingNicknameAndCategoryLast(_lastIssuedNickname) == true => (CategoryState.Donation, false),
                CategoryState.OutOfQueue when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.Debt, false),

                CategoryState.Donation when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                CategoryState.Donation when _debtCategories.HasOrderFromNewNickname(_lastIssuedNickname) => (CategoryState.Debt, false),
                CategoryState.Donation when _donationCategory?.HasOrderSkippingNicknameAndCategoryLast(_lastIssuedNickname) == true => (CategoryState.Donation, false),

                CategoryState.Debt when _outOfQueueCategory.HasOrders => (CategoryState.OutOfQueue, true),
                CategoryState.Debt when _donationCategory?.HasOrderSkippingNickname(_lastIssuedNickname) == true => (CategoryState.Donation, false),
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
        public ReviewOrderEntity TakeNextOrder(bool isOnlyNicknameLeft)
        {
            ReviewOrderEntity result = _currentState switch
            {
                CategoryState.OutOfQueue => _outOfQueueCategory.Dequeue(_lastIssuedNickname),
                CategoryState.Donation => _donationCategory!.Dequeue(_lastIssuedNickname),
                CategoryState.Debt when isOnlyNicknameLeft => _debtCategories.DequeueRoundRobin(_lastIssuedNickname),
                CategoryState.Debt when isOnlyNicknameLeft == false => _debtCategories.DequeueRoundRobinFromOtherNickname(_lastIssuedNickname),
                _ => throw new UnreachableException($"Неподдерживаемый тип категории очереди '{_currentState}'")
            };

            _lastIssuedNickname = result.MainNormalizedNickname;

            return result;
        }
    }
}