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
        /// Заказы и их позиции в очереди
        /// </summary>
        public required Dictionary<long, OrderPosition> OrderPositionsById { get; init; }

        /// <summary>
        /// Состояние менеджера приоритетов активных заказов
        /// </summary>
        public required OrderPriorityManagerState PriorityManagerState { get; init; }

        /// <summary>
        /// Возвращает текущую позицию заказа
        /// </summary>
        public OrderQueuePosition GetCurrentQueuePosition(ReviewOrder order) => OrderPositionsById[order.Id].PositionHistory.Current;

        /// <summary>
        /// Обновляет позиции заказов
        /// </summary>
        public void UpdateAllPositions()
        {
            UpdateActive();
            UpdateInProgress();
            UpdateCompleted();
            UpdateScheduled();
            UpdateFrozen();
            UpdateRemoved();
        }

        /// <summary>
        /// Обновляет заказы
        /// </summary>
        public OrderPosition[] UpdateOrders(ReviewOrder[] orders)
        {
            SaveCurrentPositionsToPrevious();

            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].UpdateOrder(order);
            }

            UpdateAllPositions();

            return GetUpdatedOrderPositions();
        }

        /// <summary>
        /// Обновляет заказ
        /// </summary>
        public OrderPosition[] UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            switch (updateType)
            {
                case OrderQueueUpdateType.OrderCreated:

                    if (NearestStreamDate == default)
                    {
                        NearestStreamDate = order.CreationStream.EventDate;
                    }

                    break;

                case OrderQueueUpdateType.OrderTaken:

                    PriorityManagerState.UpdateFromOrder(order);

                    break;

                case OrderQueueUpdateType.TrackUrlAdded:
                case OrderQueueUpdateType.OrderMovedUp:
                case OrderQueueUpdateType.OrderFrozen:
                case OrderQueueUpdateType.OrderUnfrozen:
                case OrderQueueUpdateType.OrderCompleted:
                case OrderQueueUpdateType.OrderCanceled:

                    break;

                default:
                    throw new OrderQueueException($"Тип обновления очереди '{updateType}' не поддерживается");
            }

            SaveCurrentPositionsToPrevious();

            if (updateType == OrderQueueUpdateType.OrderCreated)
            {
                OrderPosition position = OrderPosition.Create(order);
                OrderPositionsById.Add(order.Id, position);
            }
            else
            {
                OrderPosition position = OrderPositionsById[order.Id];
                position.UpdateOrder(order);
            }

            UpdateAllPositions();

            return GetUpdatedOrderPositions();
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
            OrderPriorityManager manager = new(this);

            int index = 0;
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
        /// Обновляет позиции удаленных из очереди заказов
        /// </summary>
        private void UpdateRemoved()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.Status == ReviewOrderStatus.Canceled
                    || x.ProcessingStream?.Status == ComposerStreamStatus.Completed)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Removed);
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

        /// <summary>
        /// Возвращает обновленные позиции заказов
        /// </summary>
        private OrderPosition[] GetUpdatedOrderPositions()
        {
            OrderPosition[] result = OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.IsOrderUpdated || x.PositionHistory.IsPositionChanged)
                .ToArray();

            foreach (OrderPosition position in result)
            {
                if (position.PositionHistory.Current.ActivityStatus == OrderActivityStatus.Removed)
                {
                    OrderPositionsById.Remove(position.Order.Id);
                }
            }

            return result;
        }
    }
}