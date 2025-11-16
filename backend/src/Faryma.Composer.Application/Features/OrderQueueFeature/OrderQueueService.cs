using Faryma.Composer.Application.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;
using Faryma.Composer.Application.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Application.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Repositories.Read;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory, IOrderQueueNotificationService notificationService)
    {
        private readonly SemaphoreLocker _locker = new();
        private OrderQueueManager _queueManager = null!;

        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        private int _syncVersion;

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ReviewOrder_R_Repository reviewOrder_R = new(context);
            ComposerStream_R_Repository composerStream_R = new(context);

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            ComposerStream? nearestStream = await composerStream_R.FindNearest(today);
            ReviewOrder? lastTakenOrder = await reviewOrder_R.FindLastTaken();
            ReviewOrder? lastTakenDebt = await reviewOrder_R.FindLastTakenDebt();
            ReviewOrder? lastTakenOutOfQueue = await reviewOrder_R.FindLastTakenOutOfQueue();
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await composerStream_R.GetLastNicknamesByStreamDate();
            ReviewOrder[] orders = await reviewOrder_R.GetOrdersInQueue();

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStream?.EventDate ?? today,
                OrderPositionsById = orders.ToDictionary(k => k.Id, OrderPosition.Create),
                PriorityManagerState = new OrderPriorityManagerState
                {
                    LastPriorityManagerState = (lastTakenOrder is null) ? CategoryState.Initial : OrderPriorityManagerState.MapCategoryState(lastTakenOrder.CategoryType),
                    LastIssuedNickname = lastTakenOrder?.MainNormalizedNickname,
                    LastDebtCategoryDate = lastTakenDebt?.CreationStream.EventDate,
                    LastOutOfQueueNickname = lastTakenOutOfQueue?.MainNormalizedNickname,
                    LastNicknamesByStreamDate = lastNicknamesByStreamDate,
                }
            };

            if (orders.Length > 0)
            {
                _queueManager.UpdateAllPositions();
            }
        }

        public Task<OrderQueuePosition> GetCurrentQueuePosition(ReviewOrder order) =>
            _locker.Lock(() => _queueManager.GetCurrentQueuePosition(order).Clone());

        public Task<OrderQueueSnapshot> GetQueueSnapshot() => _locker.Lock(() => new OrderQueueSnapshot
        {
            SyncVersion = _syncVersion,
            OrderQueueUpdateType = OrderQueueUpdateType.Unspecified,
            Positions = _queueManager.OrderPositionsById
                .Select(x => x.Value.Clone())
                .ToArray(),
        });

        public Task UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType) => _locker.Lock(async () =>
        {
            _syncVersion++;

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = updateType,
                Positions = _queueManager.UpdateOrder(order, updateType),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task CancelOrder(ReviewOrder order, ReviewOrderStatus previousStatus) => _locker.Lock(async () =>
        {
            _syncVersion++;

            if (previousStatus == ReviewOrderStatus.InProgress)
            {
                await using AppDbContext context = await contextFactory.CreateDbContextAsync();
                ReviewOrder_R_Repository reviewOrder_R = new(context);
                ReviewOrder? lastTakenOrder = await reviewOrder_R.FindLastTaken();
                if (lastTakenOrder is not null)
                {
                    _queueManager.PriorityManagerState.UpdateFromOrder(lastTakenOrder);
                }
            }

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.OrderCanceled,
                Positions = _queueManager.UpdateOrder(order, OrderQueueUpdateType.OrderCanceled),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task StartStream(ComposerStream stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            _queueManager.NearestStreamDate = stream.EventDate;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ReviewOrder_R_Repository reviewOrder_R = new(context);
            ReviewOrder[] orders = await reviewOrder_R.GetOrdersToStartStream(stream.Id);

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamStarted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task CompleteStream(ComposerStream stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ComposerStream_R_Repository composerStream_R = new(context);
            ComposerStream? nearestStream = await composerStream_R.FindNearest(stream.EventDate);
            if (nearestStream is not null)
            {
                _queueManager.NearestStreamDate = nearestStream.EventDate;
            }

            ReviewOrder_R_Repository reviewOrder_R = new(context);
            ReviewOrder[] orders = await reviewOrder_R.GetOrdersToCompleteStream(stream.Id);

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCompleted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });
    }
}