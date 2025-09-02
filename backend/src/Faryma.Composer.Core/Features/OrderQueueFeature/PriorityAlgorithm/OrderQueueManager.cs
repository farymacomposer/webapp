using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    /// <summary>
    /// Управляет очередью заказов
    /// </summary>
    public sealed class OrderQueueManager
    {
        /// <summary>
        /// Дата ближайшего стрима
        /// </summary>
        public required DateOnly NearestStreamDate { get; set; }

        /// <summary>
        /// Последнее состояние менеджера приоритетов
        /// </summary>
        public required CategoryState LastPriorityManagerState { get; set; }

        /// <summary>
        /// Последний обработанный никнейм
        /// </summary>
        public required string? LastIssuedNickname { get; set; }

        /// <summary>
        /// Последний никнейм в категории - вне очереди
        /// </summary>
        public required string? LastOutOfQueueNickname { get; set; }

        /// <summary>
        /// Последний никнейм в донатной и долговых категориях (по дате стрима)
        /// </summary>
        public required Dictionary<DateOnly, string> LastNicknameByStreamDate { get; init; }

        /// <summary>
        /// Заказы и их позиции в очереди
        /// </summary>
        public required Dictionary<long, OrderPosition> OrderPositionsById { get; init; }

        /// <summary>
        ///
        /// </summary>
        public static CategoryState MapCategoryState(OrderCategoryType categoryType)
        {
            return categoryType switch
            {
                OrderCategoryType.OutOfQueue => CategoryState.OutOfQueue,
                OrderCategoryType.Donation => CategoryState.Donation,
                OrderCategoryType.Debt => CategoryState.Debt,
                _ => throw new OrderQueueException($"Тип категории заказа '{categoryType}' не поддерживается")
            };
        }

        public IEnumerable<OrderPosition> UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            OrderPosition position = updateType switch
            {
                OrderQueueUpdateType.OrderCreated => AddOrder(order),
                OrderQueueUpdateType.OrderCanceled => RemoveOrder(order),
                _ => UpdateOrderInternal(order, updateType),
            };

            return OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.PositionHistory.IsPositionJumped)
                .Append(position)
                .ToHashSet();
        }

        /// <summary>
        /// Обновляет заказы
        /// </summary>
        public IEnumerable<OrderPosition> UpdateOrders(ReviewOrder[] orders)
        {
            List<OrderPosition> positions = [];
            foreach (ReviewOrder order in orders)
            {
                OrderPosition position = OrderPositionsById[order.Id];
                position.Order = order;
                positions.Add(position);
            }

            UpdateAllPositions();

            return positions;
        }

        /// <summary>
        /// Обновляет позиции заказов
        /// </summary>
        public void UpdateAllPositions()
        {
            SaveCurrentPositionsToPrevious();
            UpdateActive();
            UpdateInProgress();
            UpdateCompleted();
            UpdateScheduled();
            UpdateFrozen();
        }

        /// <summary>
        /// Добавляет заказ
        /// </summary>
        private OrderPosition AddOrder(ReviewOrder order)
        {
            OrderPosition position = OrderPosition.Create(order);
            OrderPositionsById.Add(order.Id, position);

            SaveCurrentPositionsToPrevious();
            UpdateActive();
            UpdateScheduled();

            return position;
        }

        /// <summary>
        /// Обновляет заказ
        /// </summary>
        private OrderPosition UpdateOrderInternal(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            OrderPosition position = OrderPositionsById[order.Id];
            position.Order = order;

            switch (updateType)
            {
                case OrderQueueUpdateType.TrackUrlAdded:

                    return position;

                case OrderQueueUpdateType.OrderMovedUp
                    or OrderQueueUpdateType.OrderFrozen
                    or OrderQueueUpdateType.OrderUnfrozen:

                    SaveCurrentPositionsToPrevious();
                    UpdateActive();
                    UpdateScheduled();
                    UpdateFrozen();

                    break;

                case OrderQueueUpdateType.OrderTaken:

                    LastPriorityManagerState = MapCategoryState(position.PositionHistory.Current.Category.Type);
                    SetLastNickname(order);

                    SaveCurrentPositionsToPrevious();
                    UpdateActive();
                    UpdateInProgress();

                    break;

                case OrderQueueUpdateType.OrderCompleted:

                    SaveCurrentPositionsToPrevious();
                    UpdateCompleted();

                    break;

                default:
                    throw new OrderQueueException($"Тип обновления очереди '{updateType}' не поддерживается");
            }

            return position;
        }

        /// <summary>
        /// Удаляет заказ
        /// </summary>
        private OrderPosition RemoveOrder(ReviewOrder order)
        {
            SaveCurrentPositionsToPrevious();

            OrderPosition position = OrderPositionsById[order.Id];
            OrderPositionsById.Remove(order.Id);

            UpdateActive();
            UpdateInProgress();
            UpdateScheduled();
            UpdateFrozen();

            position.PositionHistory.Current.UpdatePosition(0, OrderActivityStatus.Canceled);

            return position;
        }

        /// <summary>
        ///
        /// </summary>
        private void SetLastNickname(ReviewOrder order)
        {
            LastIssuedNickname = order.MainNickname;

            if (order.Type == ReviewOrderType.OutOfQueue)
            {
                LastOutOfQueueNickname = order.MainNickname;
            }
            else
            {
                DateOnly streamDate = order.CreationStream.EventDate;
                LastNicknameByStreamDate[streamDate] = order.MainNickname;
            }
        }

        /// <summary>
        /// Сохраняет текущие позиции заказов в предыдущее состояние (для отслеживания изменений)
        /// </summary>
        private void SaveCurrentPositionsToPrevious()
        {
            foreach (KeyValuePair<long, OrderPosition> kvp in OrderPositionsById)
            {
                kvp.Value.SaveCurrentPositionToPrevious();
            }
        }

        /// <summary>
        /// Обновляет позиции активных заказов
        /// </summary>
        private void UpdateActive()
        {
            int index = 0;
            OrderPriorityManager manager = new(this);

            while (true)
            {
                (CategoryState state, bool isOnlyNicknameLeft) = manager.DetermineNextState();
                if (state == CategoryState.Completed)
                {
                    break;
                }

                ReviewOrder order = manager.TakeNextOrder(isOnlyNicknameLeft);
                OrderPositionsById[order.Id].UpdateCurrentPosition(index, OrderActivityStatus.Active);
                index++;
            }
        }

        /// <summary>
        /// Обновляет позицию заказа в работе
        /// </summary>
        private void UpdateInProgress()
        {
            KeyValuePair<long, OrderPosition> kvp = OrderPositionsById
                .SingleOrDefault(x => x.Value.Order.Status == ReviewOrderStatus.InProgress);

            if (kvp.Value is not null)
            {
                OrderPositionsById[kvp.Value.Order.Id].UpdateCurrentPosition(0, OrderActivityStatus.InProgress);
            }
        }

        /// <summary>
        /// Обновляет позиции выполненных заказов
        /// </summary>
        private void UpdateCompleted()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.Status == ReviewOrderStatus.Completed)
                .OrderBy(x => x.CompletedAt)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Completed);
        }

        /// <summary>
        /// Обновляет позиции запланированных заказов
        /// </summary>
        private void UpdateScheduled()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen && x.CreationStream.EventDate > NearestStreamDate)
                .Order(OrderPriorityComparer.Default)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Scheduled);
        }

        /// <summary>
        /// Обновляет позиции замороженных заказов
        /// </summary>
        private void UpdateFrozen()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.IsFrozen)
                .Order(OrderPriorityComparer.Default)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Frozen);
        }

        /// <summary>
        /// Обновляет позиции заказов
        /// </summary>
        private void UpdatePositions(ReviewOrder[] orders, OrderActivityStatus activityStatus)
        {
            int index = 0;
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].UpdateCurrentPosition(index, activityStatus);
                index++;
            }
        }
    }
}