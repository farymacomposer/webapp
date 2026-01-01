using Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm;
using Faryma.Composer.Application.Utils;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Repositories.Read;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.OrderQueue
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
            ReviewOrderReadRepository reviewOrderRead = new(context);
            ComposerStreamReadRepository composerStreamRead = new(context);

            ComposerStreamEntity? nearestStream = await composerStreamRead.FindNearest();
            ReviewOrderEntity? lastTakenOrder = await reviewOrderRead.FindLastTaken();
            ReviewOrderEntity? lastTakenDebt = await reviewOrderRead.FindLastTakenDebt();
            ReviewOrderEntity? lastTakenOutOfQueue = await reviewOrderRead.FindLastTakenOutOfQueue();
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await composerStreamRead.GetLastNicknamesByStreamDate();
            List<ReviewOrderEntity> orders = await reviewOrderRead.GetOrdersInQueue();

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStream?.EventDate ?? DateOnly.MinValue,
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

            if (_queueManager.OrderPositionsById.Count > 0)
            {
                _queueManager.UpdateAllPositions();
            }
        }

        public Task<OrderQueuePosition> GetCurrentQueuePosition(ReviewOrderEntity order) =>
            _locker.Lock(() => _queueManager.GetCurrentQueuePosition(order).Clone());

        public Task<OrderQueueSnapshot> GetQueueSnapshot() => _locker.Lock(() => new OrderQueueSnapshot
        {
            SyncVersion = _syncVersion,
            OrderQueueUpdateType = OrderQueueUpdateType.Unspecified,
            Positions = _queueManager.OrderPositionsById
                .Select(x => x.Value.Clone())
                .ToArray(),
        });

        public Task UpdateOrder(ReviewOrderEntity order, OrderQueueUpdateType updateType) => _locker.Lock(async () =>
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

        public Task CancelOrder(ReviewOrderEntity order, ReviewOrderStatus previousStatus) => _locker.Lock(async () =>
        {
            _syncVersion++;

            if (previousStatus == ReviewOrderStatus.InProgress)
            {
                await using AppDbContext context = await contextFactory.CreateDbContextAsync();
                ReviewOrderReadRepository reviewOrderRead = new(context);
                ReviewOrderEntity? lastTakenOrder = await reviewOrderRead.FindLastTaken();
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

        public Task CreateStream(ComposerStreamEntity stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            _queueManager.NearestStreamDate = stream.EventDate;

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCreated,
                Positions = _queueManager.UpdateOrders(),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task StartStream(ComposerStreamEntity stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            _queueManager.NearestStreamDate = stream.EventDate;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ReviewOrderReadRepository reviewOrderRead = new(context);
            List<ReviewOrderEntity> orders = await reviewOrderRead.GetOrdersToStartStream(stream.Id);

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamStarted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task CompleteStream(ComposerStreamEntity stream) => _locker.Lock(async () =>
        {
            _syncVersion++;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ComposerStreamReadRepository composerStreamRead = new(context);
            ComposerStreamEntity? nearestStream = await composerStreamRead.FindNearest();
            _queueManager.NearestStreamDate = nearestStream?.EventDate ?? DateOnly.MinValue;

            ReviewOrderReadRepository reviewOrderRead = new(context);
            List<ReviewOrderEntity> orders = await reviewOrderRead.GetOrdersToCompleteStream(stream.Id);

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCompleted,
                Positions = _queueManager.UpdateOrders(orders),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });

        public Task CancelStream() => _locker.Lock(async () =>
        {
            _syncVersion++;

            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ComposerStreamReadRepository composerStreamRead = new(context);
            ComposerStreamEntity? nearestStream = await composerStreamRead.FindNearest();
            _queueManager.NearestStreamDate = nearestStream?.EventDate ?? DateOnly.MinValue;

            OrderQueueSnapshot snapshot = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCanceled,
                Positions = _queueManager.UpdateOrders(),
            };

            await notificationService.NotifyQueueUpdated(snapshot);
        });
    }
}