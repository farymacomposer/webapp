using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory, IOrderQueueNotificationService notificationService)
    {
        private OrderQueueManager _queueManager = null!;

        public IReadOnlyDictionary<long, OrderPosition> GetOrderQueue() => _queueManager.OrderPositionsById;

        public async Task AddOrder(ReviewOrder order)
        {
            _queueManager.AddOrder(order);

            await notificationService.NotifyNewOrderAdded(_queueManager.OrderPositionsById[order.Id]);
        }

        public async Task UpdateOrder(ReviewOrder order)
        {
            _queueManager.UpdateOrder(order);

            await notificationService.NotifyOrderPositionChanged(_queueManager.OrderPositionsById[order!.Id]);
        }

        public async Task RemoveOrder(ReviewOrder order)
        {
            OrderPosition position = _queueManager.OrderPositionsById[order!.Id];

            _queueManager.RemoveOrder(order);

            await notificationService.NotifyOrderRemoved(position);
        }

        public async Task StartReview(ReviewOrder order)
        {
            _queueManager.UpdateOrder(order);

            OrderPosition position = _queueManager.OrderPositionsById[order!.Id];

            await notificationService.NotifyReviewStarted(position);
        }

        public async Task CompleteReview(ReviewOrder order)
        {
            _queueManager.UpdateOrder(order);

            OrderPosition position = _queueManager.OrderPositionsById[order!.Id];

            await notificationService.NotifyReviewCompleted(position);
        }

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            DateOnly currentStreamDate = await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Planned || x.Status == ComposerStreamStatus.Live)
                .OrderBy(x => x.EventDate)
                .Select(x => x.EventDate)
                .FirstOrDefaultAsync();

            ReviewOrder[] orders = await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.ComposerStream)
                .Include(x => x.UserNicknames)
                .Include(x => x.Payments)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.Review != null && x.Review.ComposerStream.Status == ComposerStreamStatus.Live))
                .ToArrayAsync();

            Dictionary<long, OrderPosition> orderPositionsById = orders.ToDictionary(k => k.Id, v => new OrderPosition { Order = v });

            Dictionary<DateOnly, string> lastNicknameByStreamDate = await context.ComposerStreams
                .Where(x => x.Reviews.Count > 0
                    && x.ReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.Reviews.OrderBy(x => x.CompletedAt).Last().ReviewOrder.MainNormalizedNickname
                })
                .ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname);

            _queueManager = new OrderQueueManager
            {
                CurrentStreamDate = currentStreamDate,
                OrderPositionsById = orderPositionsById,
                LastNicknameByStreamDate = lastNicknameByStreamDate,
                LastOrderPriorityManagerState = OrderPriorityManager.State.Initial,
            };

            _queueManager.UpdateOrderPositions();
        }
    }
}