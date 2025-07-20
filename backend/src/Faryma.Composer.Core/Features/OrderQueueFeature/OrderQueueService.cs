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

        public static void Refresh(DateOnly currentStreamDate, Dictionary<long, ReviewOrder> orders, Dictionary<long, OrderPosition> orderPositions)
        {
            if (orders.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<long, OrderPosition> item in orderPositions)
            {
                item.Value.Prev = item.Value.Current;
            }

            int index = 0;
            OrderPriorityManager manager = new(currentStreamDate, orders);
            while (manager.DetermineNextState() != OrderPriorityManager.State.Completed)
            {
                ReviewOrder order = manager.TakeNextOrder();
                orderPositions[order.Id].Current = index;
                index++;
            }

            ReviewOrder[] inactiveOrders = orders
                .Select(x => x.Value)
                .Where(x => !x.IsActive)
                .OrderBy(x => x.CreatedAt)
                .ToArray();

            foreach (ReviewOrder order in inactiveOrders)
            {
                orderPositions[order.Id].Current = index;
                index++;
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

        public void Up(Transaction payment)
        {
            _orders[payment.ReviewOrder!.Id].Payments.Add(payment);
            Refresh(_currentStreamDate, _orders, _orderPositions);
        }
    }
}