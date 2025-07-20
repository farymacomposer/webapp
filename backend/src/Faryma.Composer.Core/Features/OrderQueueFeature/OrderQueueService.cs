using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory)
    {
        private Dictionary<long, OrderQueuePosition> _orderPositions = null!;
        private Dictionary<long, ReviewOrder> _orders = null!;
        private DateOnly _currentStreamDate;

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

            _orderPositions = _orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(_currentStreamDate, _orders, _orderPositions);
        }

        public void Add(ReviewOrder order)
        {
            _orders.Add(order.Id, order);
            Algorithm.RefreshOrderPositions(_currentStreamDate, _orders, _orderPositions);
        }

        // пауза
        public void Up(Transaction payment)
        {
            _orders[payment.ReviewOrder!.Id].Payments.Add(payment);
            Algorithm.RefreshOrderPositions(_currentStreamDate, _orders, _orderPositions);
        }
    }
}