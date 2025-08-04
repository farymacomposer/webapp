using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;

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

        /// <summary>
        /// Добавляет заказ
        /// </summary>
        public void AddOrder(ReviewOrder order)
        {
            OrderPositionsById.Add(order.Id, new OrderPosition { Order = order });
            UpdateOrderPositions();
        }

        /// <summary>
        /// Обновляет заказ
        /// </summary>
        public void UpdateOrder(ReviewOrder order)
        {
            OrderPositionsById[order.Id].Order = order;
            UpdateOrderPositions();
        }

        /// <summary>
        /// Обновляет позиции заказов
        /// </summary>
        public void UpdateOrderPositions()
        {
            SwapOrderPositions();
            UpdateScheduledOrdersPositions();
            UpdateActiveOrdersPositions();
            UpdateFrozenOrdersPositions();
        }

        private void SwapOrderPositions()
        {
            foreach (KeyValuePair<long, OrderPosition> item in OrderPositionsById)
            {
                item.Value.Swap();
            }
        }

        private void UpdateScheduledOrdersPositions()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => !x.IsFrozen && x.ComposerStream.EventDate > CurrentStreamDate)
                .Order(new OrderPriorityComparer())
                .ToArray();

            int index = 0;
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].SetCurrentPosition(index, OrderActivityStatus.Scheduled);
                index++;
            }
        }

        private void UpdateActiveOrdersPositions()
        {
            int index = 0;
            OrderPriorityManager manager = new(this);
            manager.UpdateOrdersCategories();

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

        private void UpdateFrozenOrdersPositions()
        {
            ReviewOrder[] orders = OrderPositionsById
                .Select(x => x.Value.Order)
                .Where(x => x.IsFrozen)
                .Order(new OrderPriorityComparer())
                .ToArray();

            int index = 0;
            foreach (ReviewOrder order in orders)
            {
                OrderPositionsById[order.Id].SetCurrentPosition(index, OrderActivityStatus.Frozen);
                index++;
            }
        }
    }
}