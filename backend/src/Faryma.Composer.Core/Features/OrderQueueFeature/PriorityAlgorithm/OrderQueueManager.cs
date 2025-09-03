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
        public required Dictionary<DateOnly, string> LastNicknamesByStreamDate { get; init; }

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

        public OrderPosition[] StartStream(ReviewOrder[] orders)
        {
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].Order = order;
            }

            SaveCurrentPositionsToPrevious();
            UpdateActive();
            UpdateScheduled();
            UpdateFrozen();

            OrderPosition[] result = OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.PositionHistory.IsStatusChanged
                    || x.PositionHistory.IsPositionJumped
                    || orders.Contains(x.Order))
                .ToArray();

            return result;
        }

        public OrderPosition[] CompleteStream(ReviewOrder[] orders)
        {
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].Order = order;
            }

            SaveCurrentPositionsToPrevious();
            UpdateActive();
            UpdateScheduled();
            UpdateFrozen();
            UpdateRemoved();

            OrderPosition[] result = OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.PositionHistory.IsStatusChanged
                    || x.PositionHistory.IsPositionJumped
                    || orders.Contains(x.Order))
                .ToArray();

            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById.Remove(order.Id);
            }

            return result;
        }

        /// <summary>
        /// Обновляет заказ
        /// </summary>
        public OrderPosition[] UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            OrderPosition position;
            if (updateType == OrderQueueUpdateType.OrderCreated)
            {
                position = OrderPosition.Create(order);
                OrderPositionsById.Add(order.Id, position);
            }
            else
            {
                position = OrderPositionsById[order.Id];
                position.Order = order;
            }

            switch (updateType)
            {
                case OrderQueueUpdateType.OrderCreated:

                    SaveCurrentPositionsToPrevious();
                    UpdateActive();
                    UpdateScheduled();

                    break;

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

                case OrderQueueUpdateType.OrderCanceled:

                    SaveCurrentPositionsToPrevious();
                    UpdateActive();
                    UpdateScheduled();
                    UpdateFrozen();
                    position.UpdateCurrentPosition(0, OrderActivityStatus.Removed);

                    OrderPositionsById.Remove(order.Id);

                    break;

                case OrderQueueUpdateType.TrackUrlAdded:

                    break;

                default:
                    throw new OrderQueueException($"Тип обновления очереди '{updateType}' не поддерживается");
            }

            OrderPosition[] active = OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.PositionHistory.Current.ActivityStatus == OrderActivityStatus.Active)
                .OrderBy(x => x.PositionHistory.Current.QueueIndex)
                .ToArray();

            List<OrderPosition> swaps = [];

            for (int i = 0; i < active.Length - 1; i++)
            {
                OrderPosition current = active[i];
                OrderPosition next = active[i + 1];

                if (current.PositionHistory.Previous.QueueIndex == next.PositionHistory.Current.QueueIndex
                    && current.PositionHistory.Current.QueueIndex == next.PositionHistory.Previous.QueueIndex)
                {
                    swaps.Add(current);
                    swaps.Add(next);
                    i++;
                }
            }

            return OrderPositionsById
                .Select(x => x.Value)
                .Where(x => x.PositionHistory.IsStatusChanged || x.PositionHistory.IsPositionJumped)
                .Concat(swaps)
                .Append(position)
                .DistinctBy(x => x.Order.Id)
                .ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        public void SetLastNickname(ReviewOrder order)
        {
            LastIssuedNickname = order.MainNormalizedNickname;

            if (order.Type == ReviewOrderType.OutOfQueue)
            {
                LastOutOfQueueNickname = order.MainNormalizedNickname;
            }
            else
            {
                DateOnly streamDate = order.CreationStream.EventDate;
                LastNicknamesByStreamDate[streamDate] = order.MainNormalizedNickname;
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
                .Where(x => x.ProcessingStream?.Status == ComposerStreamStatus.Completed)
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
    }
}