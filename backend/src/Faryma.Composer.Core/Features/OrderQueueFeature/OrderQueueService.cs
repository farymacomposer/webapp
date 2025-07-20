using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory)
    {
        private Dictionary<long, OrderPosition> _orderPositions = null!;
        private Dictionary<long, ReviewOrder> _orders = null!;
        private DateOnly _currentStreamDate;

        public static void Refresh(
            DateOnly currentStreamDate,
            Dictionary<long, ReviewOrder> orders,
            Dictionary<long, OrderPosition> orderPositions)
        {
            if (orders.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<long, OrderPosition> item in orderPositions)
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

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            _currentStreamDate = await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Planned)
                .OrderBy(x => x.EventDate)
                .Select(x => x.EventDate)
                .FirstOrDefaultAsync();

            _orders = await context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending)
                .Include(x => x.ComposerStream)
                .Include(x => x.UserNickname)
                .Include(x => x.Payments)
                .ToDictionaryAsync(k => k.Id);

            _orderPositions = _orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            Refresh(_currentStreamDate, _orders, _orderPositions);
        }

        public void Add(ReviewOrder order)
        {
            _orders.Add(order.Id, order);
            Refresh(_currentStreamDate, _orders, _orderPositions);
        }

        // пауза
        public void Up(Transaction payment)
        {
            _orders[payment.ReviewOrder!.Id].Payments.Add(payment);
            Refresh(_currentStreamDate, _orders, _orderPositions);
        }
    }
}