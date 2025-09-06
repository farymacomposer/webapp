using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Core.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Repositories;
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
            ReviewOrderRepository reviewOrderRepository = new(context);
            ComposerStreamRepository composerStreamRepository = new(context);

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            ComposerStream? nearestStream = await composerStreamRepository.FindNearest(today);
            ReviewOrder? lastOrder = await reviewOrderRepository.FindLastCompleted();
            string? lastOutOfQueueNickname = await reviewOrderRepository.FindLastOutOfQueueNickname();
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await composerStreamRepository.GetLastNicknamesByStreamDate();
            ReviewOrder[] orders = await reviewOrderRepository.GetOrdersInQueue();

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStream?.EventDate ?? today,
                LastPriorityManagerState = (lastOrder is null) ? CategoryState.Initial : OrderQueueManager.MapCategoryState(lastOrder.CategoryType),
                LastIssuedNickname = lastOrder?.MainNormalizedNickname,
                LastOutOfQueueNickname = lastOutOfQueueNickname,
                LastNicknamesByStreamDate = lastNicknamesByStreamDate,
                OrderPositionsById = orders.ToDictionary(k => k.Id, OrderPosition.Create),
            };

            if (orders.Length > 0)
            {
                _queueManager.UpdateAllPositions();
            }
        }

        public Task<OrderQueuePosition> GetCurrentQueuePosition(ReviewOrder order) =>
            _locker.Lock(() => _queueManager.GetCurrentQueuePosition(order));

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
                ReviewOrderRepository reviewOrderRepository = new(context);
                ReviewOrder? lastCompleted = await reviewOrderRepository.FindLastCompleted();
                if (lastCompleted is not null)
                {
                    _queueManager.LastPriorityManagerState = OrderQueueManager.MapCategoryState(lastCompleted.CategoryType);
                    _queueManager.SetLastNickname(lastCompleted);
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
            ReviewOrderRepository reviewOrderRepository = new(context);
            ReviewOrder[] orders = await reviewOrderRepository.GetOrdersToStartStream(stream.Id);

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
            ComposerStreamRepository composerStreamRepository = new(context);
            ComposerStream? nearestStream = await composerStreamRepository.FindNearest(stream.EventDate);
            if (nearestStream is not null)
            {
                _queueManager.NearestStreamDate = nearestStream.EventDate;
            }

            ReviewOrderRepository reviewOrderRepository = new(context);
            ReviewOrder[] orders = await reviewOrderRepository.GetOrdersToCompleteStream(stream.Id);

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