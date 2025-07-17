using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory)
    {
        private Dictionary<long, TestC> _queueOrder = null!;
        private Dictionary<long, ReviewOrder> _orders = null!;

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            _orders = await context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending)
                .Include(x => x.ComposerStream)
                .Include(x => x.UserNickname)
                .Include(x => x.Payments)
                .ToDictionaryAsync(k => k.Id);

            _queueOrder = _orders.ToDictionary(k => k.Key, _ => new TestC());

            Refresh();
        }

        public void Add(ReviewOrder order)
        {
            _orders.Add(order.Id, order);
            Refresh();
        }

        public void Up(Transaction payment)
        {
            _orders[payment.ReviewOrder!.Id].Payments.Add(payment);
            Refresh();
        }

        private void Refresh()
        {
            Queue<ReviewOrder> outOfQueue = new(_orders
                .Select(x => x.Value)
                .Where(x => x.IsActive && x.Type == ReviewOrderType.OutOfQueue)
                .OrderBy(x => x.CreatedAt));

            List<(DateOnly Key, PriorityQueue<ReviewOrder, ReviewOrder> Queue)> items = _orders
                .Select(x => x.Value)
                .Where(x => x.IsActive && x.Type is ReviewOrderType.Donation or ReviewOrderType.Free)
                .GroupBy(x => x.ComposerStream.EventDate)
                .Select(x => (x.Key, new PriorityQueue<ReviewOrder, ReviewOrder>(x.Select(x => (x, x)), new ReviewOrderComparer())))
                .OrderBy(x => x.Key)
                .ToList();

            int index = 0;
            var prevNickname = "";

            while (outOfQueue.Count > 0 || items.Count > 0)
            {
                if (outOfQueue.Count > 0)
                {
                    ReviewOrder order = outOfQueue.Dequeue();
                    if (order.UserNickname.NormalizedNickname != prevNickname)
                    {
                        prevNickname = order.UserNickname.NormalizedNickname;
                        _queueOrder[order.Id].Current = index;
                        index++;

                        continue;
                    }
                }

                if (items.Count > 0)
                {
                }

                index++;
            }

            ReviewOrder[] isNotActive = _orders
                .Select(x => x.Value)
                .Where(x => !x.IsActive)
                .OrderBy(x => x.CreatedAt)
                .ToArray();

            foreach (ReviewOrder order in isNotActive)
            {
                _queueOrder[order.Id].Current = index;
                index++;
            }
        }
    }

    public sealed class TestC
    {
        public int Prev { get; set; }
        public int Current { get; set; }
    }
}