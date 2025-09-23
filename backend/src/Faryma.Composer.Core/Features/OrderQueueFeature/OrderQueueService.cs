using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Core.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Repositories.Read;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
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
            ReviewOrder? lastTakenDebtOrder = await reviewOrder_R.FindLastTakenDebt();
            string? lastOutOfQueueNickname = await reviewOrder_R.FindLastTakenOutOfQueueNickname();
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
                    LastDebtCategoryDate = lastTakenDebtOrder?.CreationStream.EventDate,
                    LastOutOfQueueNickname = lastOutOfQueueNickname,
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

        public Task<OrderQueue> GetOrderQueue() => _locker.Lock(() => new OrderQueue
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

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = updateType,
                Positions = _queueManager.UpdateOrder(order, updateType),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
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

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.OrderCanceled,
                Positions = _queueManager.UpdateOrder(order, OrderQueueUpdateType.OrderCanceled),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
        });

        public Task StartStream(ComposerStream stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            _queueManager.NearestStreamDate = stream.EventDate;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ReviewOrder_R_Repository reviewOrder_R = new(context);
            ReviewOrder[] orders = await reviewOrder_R.GetOrdersToStartStream(stream.Id);

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamStarted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
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

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCompleted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
        });
    }
}