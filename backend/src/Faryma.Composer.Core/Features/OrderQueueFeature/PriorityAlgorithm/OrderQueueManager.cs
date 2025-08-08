using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public sealed class OrderQueueManager
    {
        /// <summary>
        /// Дата текущего стрима
        /// </summary>
        public required DateOnly CurrentStreamDate { get; set; }

        /// <summary>
        ///
        /// </summary>
        public required OrderPriorityManager.State LastOrderPriorityManagerState { get; set; }

        /// <summary>
        ///
        /// </summary>
        public required Dictionary<long, OrderPosition> OrderPositionsById { get; init; }

        /// <summary>
        /// Последний никнейм в категории (по дате стрима)
        /// </summary>
        public required Dictionary<DateOnly, string> LastNicknameByStreamDate { get; init; }

        public required string? LastOutOfQueueCategoryNickname { get; set; }

        /// <summary>
        /// Добавляет заказ
        /// </summary>
        public void AddOrder(ReviewOrder order)
        {
            OrderPositionsById.Add(order.Id, new OrderPosition { Order = order });

            SwapOrderPositions();
            UpdateActive();
            UpdateScheduled();
        }

        /// <summary>
        /// Обновляет заказ
        /// </summary>
        public void UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            OrderPosition position = OrderPositionsById[order.Id];
            position.Order = order;

            switch (updateType)
            {
                case OrderQueueUpdateType.AddTrackUrl:

                    return;

                case OrderQueueUpdateType.Up or OrderQueueUpdateType.Freeze or OrderQueueUpdateType.Unfreeze:

                    SwapOrderPositions();
                    UpdateActive();
                    UpdateScheduled();
                    UpdateFrozen();

                    break;

                case OrderQueueUpdateType.TakeInProgress:

                    SetManagerState(position);
                    SetLastNickname(order);

                    SwapOrderPositions();
                    UpdateActive();
                    UpdateInProgress();

                    break;

                case OrderQueueUpdateType.Complete:

                    SwapOrderPositions();
                    UpdateCompleted();

                    break;

                default:
                    throw new OrderQueueException($"Тип обновления очереди '{updateType}' не поддерживается");
            }
        }

        /// <summary>
        /// Удаляет заказ
        /// </summary>
        public void RemoveOrder(ReviewOrder order)
        {
            OrderPositionsById.Remove(order.Id);

            SwapOrderPositions();
            UpdateActive();
            UpdateInProgress();
            UpdateScheduled();
            UpdateFrozen();
        }

        /// <summary>
        /// Обновляет позиции заказов
        /// </summary>
        public void UpdateOrderPositions()
        {
            SwapOrderPositions();
            UpdateActive();
            UpdateInProgress();
            UpdateCompleted();
            UpdateScheduled();
            UpdateFrozen();
        }

        private void SetManagerState(OrderPosition position)
        {
            OrderCategoryType categoryType = position.PositionHistory.Current.Category.Type;

            LastOrderPriorityManagerState = categoryType switch
            {
                OrderCategoryType.OutOfQueue => OrderPriorityManager.State.OutOfQueueCategory,
                OrderCategoryType.Donation => OrderPriorityManager.State.DonationCategory,
                OrderCategoryType.Debt => OrderPriorityManager.State.DebtCategories,
                _ => throw new OrderQueueException($"Тип категории заказа '{categoryType}' не поддерживается")
            };
        }

        private void SetLastNickname(ReviewOrder order)
        {
            if (order.Type == ReviewOrderType.OutOfQueue)
            {
                LastOutOfQueueCategoryNickname = order.MainNickname;
            }
            else
            {
                DateOnly streamDate = order.CreationStream.EventDate;
                LastNicknameByStreamDate[streamDate] = order.MainNickname;
            }
        }

        private void SwapOrderPositions()
        {
            foreach (KeyValuePair<long, OrderPosition> item in OrderPositionsById)
            {
                item.Value.Swap();
            }
        }

        private void UpdateActive()
        {
            int index = 0;
            OrderPriorityManager manager = new(this);

            while (true)
            {
                (OrderPriorityManager.State state, bool isOnlyNicknameLeft) = manager.DetermineNextState();
                if (state == OrderPriorityManager.State.Completed)
                {
                    break;
                }

                ReviewOrder order = manager.TakeNextOrder(isOnlyNicknameLeft);
                OrderPositionsById[order.Id].SetCurrentPosition(index, OrderActivityStatus.Active);
                index++;
            }
        }

        private void UpdateInProgress()
        {
            KeyValuePair<long, OrderPosition> kvp = OrderPositionsById
                .FirstOrDefault(x => x.Value.Order.Status == ReviewOrderStatus.InProgress);

            if (kvp.Value is not null)
            {
                OrderPositionsById[kvp.Value.Order.Id].SetCurrentPosition(0, OrderActivityStatus.InProgress);
            }
        }

        private void UpdateCompleted()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.Status == ReviewOrderStatus.Completed)
                .OrderBy(x => x.CompletedAt)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Completed);
        }

        private void UpdateScheduled()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen && x.CreationStream.EventDate > CurrentStreamDate)
                .Order(OrderPriorityComparer.Default)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Scheduled);
        }

        private void UpdateFrozen()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.IsFrozen)
                .Order(OrderPriorityComparer.Default)
                .ToArray();

            UpdatePositions(orders, OrderActivityStatus.Frozen);
        }

        private void UpdatePositions(ReviewOrder[] orders, OrderActivityStatus activityStatus)
        {
            int index = 0;
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].SetCurrentPosition(index, activityStatus);
                index++;
            }
        }
    }
}