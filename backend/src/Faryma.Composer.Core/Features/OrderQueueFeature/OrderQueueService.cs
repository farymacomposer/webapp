using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Core.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory, IOrderQueueNotificationService notificationService)
    {
        private readonly SemaphoreLocker _locker = new();
        private OrderQueueManager _queueManager = null!;
        private int _positionsHashCode;

        public Task<OrderPosition[]> GetOrderQueue() =>
            _locker.Lock(() => _queueManager.OrderPositionsById.Select(x => x.Value.Clone()).ToArray());

        public Task<OrderQueuePosition> GetCurrentQueuePosition(ReviewOrder order) =>
            _locker.Lock(() => _queueManager.OrderPositionsById[order.Id].PositionHistory.Current.Clone());

        public async Task AddOrder(ReviewOrder order)
        {
            await _locker.Lock(async () =>
            {
                int positionsHashCode = _positionsHashCode;
                OrderPosition position = _queueManager.AddOrder(order);
                _positionsHashCode = DateTime.UtcNow.GetHashCode();

                await notificationService.NotifyNewOrderAdded(positionsHashCode, position);
            });
        }

        public async Task UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType)
        {
            await _locker.Lock(async () =>
            {
                int positionsHashCode = _positionsHashCode;
                OrderPosition position = _queueManager.UpdateOrder(order, updateType);
                _positionsHashCode = DateTime.UtcNow.GetHashCode();

                await notificationService.NotifyOrderPositionChanged(positionsHashCode, position, updateType);
            });
        }

        public async Task UpdateOrders(ReviewOrder[] orders)
        {
            await _locker.Lock(async () =>
            {
                int positionsHashCode = _positionsHashCode;
                IEnumerable<OrderPosition> positions = _queueManager.UpdateOrders(orders);
                _positionsHashCode = DateTime.UtcNow.GetHashCode();

                await notificationService.NotifyOrderPositionsChanged(positionsHashCode, positions);
            });
        }

        public async Task RemoveOrder(ReviewOrder order)
        {
            await _locker.Lock(async () =>
            {
                int positionsHashCode = _positionsHashCode;
                OrderPosition position = _queueManager.RemoveOrder(order);
                _positionsHashCode = DateTime.UtcNow.GetHashCode();

                await notificationService.NotifyOrderRemoved(positionsHashCode, position);
            });
        }

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            DateOnly nearestStreamDate = await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live || x.Status == ComposerStreamStatus.Planned)
                .OrderBy(x => x.EventDate)
                .Select(x => x.EventDate)
                .FirstOrDefaultAsync();

            ReviewOrder? lastOrder = await context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Status == ReviewOrderStatus.InProgress || x.Status == ReviewOrderStatus.Completed)
                .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                .LastOrDefaultAsync();

            string? lastOutOfQueueNickname = await context.ReviewOrders
                .Where(x => x.Type == ReviewOrderType.OutOfQueue
                    && (x.Status == ReviewOrderStatus.InProgress || x.Status == ReviewOrderStatus.Completed))
                .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                .Select(x => x.MainNormalizedNickname)
                .LastOrDefaultAsync();

            Dictionary<DateOnly, string> lastNicknameByStreamDate = await context.ComposerStreams
                .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                    && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.ProcessedReviewOrders.Where(x => x.Type != ReviewOrderType.OutOfQueue)
                        .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                        .Last().MainNormalizedNickname
                })
                .ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname);

            ReviewOrder[] orders = await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.Payments)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.ProcessingStream != null && x.ProcessingStream.Status == ComposerStreamStatus.Live))
                .ToArrayAsync();

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStreamDate,
                LastPriorityManagerState = (lastOrder is null) ? CategoryState.Initial : OrderQueueManager.MapCategoryState(lastOrder.CategoryType),
                LastIssuedNickname = lastOrder?.MainNormalizedNickname,
                LastOutOfQueueNickname = lastOutOfQueueNickname,
                LastNicknameByStreamDate = lastNicknameByStreamDate,
                OrderPositionsById = orders.ToDictionary(k => k.Id, v => new OrderPosition { Order = v }),
            };

            if (orders.Length > 0)
            {
                _queueManager.UpdateAllPositions();
            }
        }
    }
}