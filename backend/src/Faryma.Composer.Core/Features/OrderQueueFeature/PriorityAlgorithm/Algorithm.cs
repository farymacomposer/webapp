using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    public static class Algorithm
    {
        public static void RefreshOrderPositions(
            DateOnly currentStreamDate,
            Dictionary<long, ReviewOrder> orders,
            Dictionary<long, OrderQueuePosition> orderPositions)
        {
            if (orders.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<long, OrderQueuePosition> item in orderPositions)
            {
                item.Value.PrevIndex = item.Value.CurrentIndex;
                item.Value.PrevActivityStatus = item.Value.CurrentActivityStatus;
            }

            ReviewOrder[] futureOrders = orders
                .Select(x => x.Value)
                .Where(x => x.IsActive && x.ComposerStream.EventDate > currentStreamDate)
                .Order(new ReviewOrderComparer())
                .ToArray();

            int futureIndex = 0;
            foreach (ReviewOrder order in futureOrders)
            {
                orderPositions[order.Id].CurrentIndex = futureIndex;
                orderPositions[order.Id].CurrentActivityStatus = OrderActivityStatus.Future;
                futureIndex++;
            }

            int activeIndex = 0;
            OrderPriorityManager manager = new(currentStreamDate, orders);
            while (true)
            {
                (OrderPriorityManager.State state, bool isOnlyNicknameLeft) = manager.DetermineNextState();
                if (state == OrderPriorityManager.State.Completed)
                {
                    break;
                }

                ReviewOrder order = manager.TakeNextOrder(isOnlyNicknameLeft);
                orderPositions[order.Id].CurrentIndex = activeIndex;
                orderPositions[order.Id].CurrentActivityStatus = OrderActivityStatus.Active;
                activeIndex++;
            }

            ReviewOrder[] inactiveOrders = orders
                .Select(x => x.Value)
                .Where(x => !x.IsActive)
                .Order(new ReviewOrderComparer())
                .ToArray();

            int inactiveIndex = 0;
            foreach (ReviewOrder order in inactiveOrders)
            {
                orderPositions[order.Id].CurrentIndex = inactiveIndex;
                orderPositions[order.Id].CurrentActivityStatus = OrderActivityStatus.Inactive;
                inactiveIndex++;
            }
        }
    }
}