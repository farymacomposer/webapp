using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory, INotificationService notificationService)
    {
        private OrderQueueManager _queueManager = null!;

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            DateOnly currentStreamDate = await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Planned || x.Status == ComposerStreamStatus.Live)
                .OrderBy(x => x.EventDate)
                .Select(x => x.EventDate)
                .FirstOrDefaultAsync();

            Dictionary<long, ReviewOrder> ordersById = await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.ComposerStream)
                .Include(x => x.UserNickname)
                .Include(x => x.Payments)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.Review != null && x.Review.ComposerStream.Status == ComposerStreamStatus.Live))
                .ToDictionaryAsync(k => k.Id);

            Dictionary<long, OrderPositionTracker> orderPositionsById = ordersById.ToDictionary(k => k.Key, _ => new OrderPositionTracker());

            Dictionary<DateOnly, string> lastNicknameByStreamDate = await context.ComposerStreams
                .Where(x => x.Reviews.Count > 0
                    && x.ReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.Reviews.OrderBy(x => x.CompletedAt).Last().ReviewOrder.UserNickname.NormalizedNickname
                })
                .ToDictionaryAsync(k => k.EventDate, v => v.NormalizedNickname);

            _queueManager = new OrderQueueManager
            {
                CurrentStreamDate = currentStreamDate,
                OrdersById = ordersById,
                OrderPositionsById = orderPositionsById,
                LastNicknameByStreamDate = lastNicknameByStreamDate,
                LastOrderPriorityManagerState = OrderPriorityManager.State.Initial,
            };

            _queueManager.UpdateOrderPositions();
        }

        public async Task Add(ReviewOrder order)
        {
            _queueManager.Add(order);
            _queueManager.UpdateOrderPositions();

            await notificationService.SendOrderPosition(_queueManager.OrderPositionsById[order.Id]);
        }

        public async Task Up(Transaction payment)
        {
            _queueManager.Up(payment);
            _queueManager.UpdateOrderPositions();

            await notificationService.SendOrderPosition(_queueManager.OrderPositionsById[payment.ReviewOrderId!.Value]);
        }

        public IReadOnlyCollection<OrderQueueItem> GetOrderQueue() => _queueManager.GetOrderQueue();
    }
}