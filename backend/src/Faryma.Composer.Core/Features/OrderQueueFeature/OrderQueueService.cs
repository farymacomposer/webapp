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
            _locker.Lock(() => _queueManager.OrderPositionsById[order.Id].PositionHistory.Current.Clone());

        public Task<OrderQueue> GetOrderQueue() => _locker.Lock(() => new OrderQueue
        {
            SyncVersion = _syncVersion,
            OrderQueueUpdateType = OrderQueueUpdateType.Unspecified,
            Positions = _queueManager.OrderPositionsById
                .Select(x => x.Value.Clone())
                .ToArray(),
        });

        public Task UpdateOrder(ReviewOrder order, OrderQueueUpdateType updateType, ReviewOrderStatus lastStatus = ReviewOrderStatus.Unspecified) => _locker.Lock(async () =>
        {
            _syncVersion++;

            if (updateType == OrderQueueUpdateType.OrderCreated && _queueManager.NearestStreamDate == default)
            {
                _queueManager.NearestStreamDate = order.CreationStream.EventDate;
            }

            if (updateType == OrderQueueUpdateType.OrderCanceled && lastStatus == ReviewOrderStatus.InProgress)
            {
                await using AppDbContext context = await contextFactory.CreateDbContextAsync();
                ReviewOrderRepository reviewOrderRepository = new(context);
                ReviewOrder? last = await reviewOrderRepository.FindLastCompleted();
                if (last is not null)
                {
                    _queueManager.SetLastNickname(last);
                }
            }

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = updateType,
                Positions = _queueManager.UpdateOrder(order, updateType),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
        });

        public Task StartStream(ComposerStream creationStream, ReviewOrder[] orders) => _locker.Lock(async () =>
        {
            _syncVersion++;

            _queueManager.NearestStreamDate = creationStream.EventDate;

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamStarted,
                Positions = _queueManager.StartStream(orders),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
        });

        public Task CompleteStream(ComposerStream? nearestStream, ReviewOrder[] orders) => _locker.Lock(async () =>
        {
            _syncVersion++;

            if (nearestStream is not null)
            {
                _queueManager.NearestStreamDate = nearestStream.EventDate;
            }

            OrderQueue orderQueue = new()
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = OrderQueueUpdateType.StreamCompleted,
                Positions = _queueManager.CompleteStream(orders),
            };

            await notificationService.NotifyOrderPositionsChanged(orderQueue);
        });
    }
}