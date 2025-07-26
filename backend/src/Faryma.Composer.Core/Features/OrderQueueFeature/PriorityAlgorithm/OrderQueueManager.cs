using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

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
        /// Заказы на разбор
        /// </summary>
        public required Dictionary<long, ReviewOrder> OrdersById { get; init; }

        /// <summary>
        /// Позиции заказов в очереди на разбор
        /// </summary>
        public required Dictionary<long, OrderPositionTracker> OrderPositionsById { get; init; }

        /// <summary>
        /// Последний никнейм в категории (по дате стрима)
        /// </summary>
        public required Dictionary<DateOnly, string> LastNicknameByStreamDate { get; init; }

        public static async Task<OrderQueueManager> CreateFromDatabase(AppDbContext context)
        {
            DateOnly currentStreamDate = await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Planned)
                .OrderBy(x => x.EventDate)
                .Select(x => x.EventDate)
                .FirstOrDefaultAsync();

            Dictionary<long, ReviewOrder> ordersById = await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.ComposerStream)
                .Include(x => x.UserNickname)
                .Include(x => x.Payments)
                .Where(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending)
                .ToDictionaryAsync(k => k.Id);

            Dictionary<long, OrderPositionTracker> orderPositionsById = ordersById.ToDictionary(k => k.Key, _ => new OrderPositionTracker());

            Dictionary<DateOnly, string> lastNicknameByStreamDate = await context.ComposerStreams
                .Where(x => x.Reviews.Count > 0
                    && x.ReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new { x.EventDate, x.Reviews.OrderBy(x => x.CompletedAt).Last().ReviewOrder.UserNickname.NormalizedNickname })
                .ToDictionaryAsync(k => k.EventDate, v => v.NormalizedNickname);

            return new OrderQueueManager
            {
                CurrentStreamDate = currentStreamDate,
                OrdersById = ordersById,
                OrderPositionsById = orderPositionsById,
                LastNicknameByStreamDate = lastNicknameByStreamDate,
                LastOrderPriorityManagerState = OrderPriorityManager.State.Initial,
            };
        }

        public void Up(Transaction payment) => OrdersById[payment.ReviewOrder!.Id].Payments.Add(payment);

        public void Add(ReviewOrder order)
        {
            OrdersById.Add(order.Id, order);
            OrderPositionsById.Add(order.Id, new OrderPositionTracker());
        }

        public void UpdateOrderPositions()
        {
            if (OrdersById.Count == 0)
            {
                return;
            }

            SwapOrderPositions();
            UpdateFutureOrdersPositions();
            UpdateActiveOrdersPositions();
            UpdateInactiveOrdersPositions();
        }

        public IReadOnlyCollection<OrderQueueItem> GetOrderQueue()
        {
            return OrdersById
                .Select(x => new OrderQueueItem
                {
                    Order = OrdersById[x.Key],
                    Position = OrderPositionsById[x.Key].Current,
                })
                .ToArray();
        }

        private void SwapOrderPositions()
        {
            foreach (KeyValuePair<long, OrderPositionTracker> item in OrderPositionsById)
            {
                item.Value.Previous.Swap(item.Value.Current);
            }
        }

        private void UpdateFutureOrdersPositions()
        {
            ReviewOrder[] futureOrders = OrdersById
                .Select(x => x.Value)
                .Where(x => x.IsActive && x.ComposerStream.EventDate > CurrentStreamDate)
                .Order(new OrderPriorityComparer())
                .ToArray();

            int index = 0;
            foreach (ReviewOrder order in futureOrders)
            {
                OrderPositionsById[order.Id].Current.Set(index, OrderActivityStatus.Future);
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
                OrderPositionsById[order.Id].Current.Set(index, OrderActivityStatus.Active);
                index++;
            }
        }

        private void UpdateInactiveOrdersPositions()
        {
            ReviewOrder[] inactiveOrders = OrdersById
                .Select(x => x.Value)
                .Where(x => !x.IsActive)
                .Order(new OrderPriorityComparer())
                .ToArray();

            int index = 0;
            foreach (ReviewOrder order in inactiveOrders)
            {
                OrderPositionsById[order.Id].Current.Set(index, OrderActivityStatus.Inactive);
                index++;
            }
        }
    }
}