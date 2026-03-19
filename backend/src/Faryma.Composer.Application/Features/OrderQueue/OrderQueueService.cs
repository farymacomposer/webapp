using System.Diagnostics;
using Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm;
using Faryma.Composer.Application.Utils;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Persistence.Queries;
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
            ReviewOrderQueries reviewOrderQueries = new(context);
            ComposerStreamQueries composerStreamQueries = new(context);

            DateOnly nearestStreamDate = await GetNearestStreamDate(context);
            ReviewOrderEntity? lastTakenOrder = await reviewOrderQueries.FindLastTaken(CancellationToken.None);
            ReviewOrderEntity? lastTakenDebt = await reviewOrderQueries.FindLastTakenDebt(CancellationToken.None);
            ReviewOrderEntity? lastTakenOutOfQueue = await reviewOrderQueries.FindLastTakenOutOfQueue(CancellationToken.None);
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await composerStreamQueries.GetLastNicknamesByStreamDate(CancellationToken.None);
            List<ReviewOrderEntity> orders = await reviewOrderQueries.GetOrdersInQueue(CancellationToken.None);

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStreamDate,
                OrderPositionsById = orders.ToDictionary(k => k.Id, OrderPosition.Create),
                PriorityManagerState = new OrderPriorityManagerState
                {
                    LastPriorityManagerState = (lastTakenOrder is null)
                        ? CategoryState.Initial
                        : OrderPriorityManagerState.MapCategoryState(lastTakenOrder.CategoryType),
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

        public Task HandleEvent(OrderQueueEvent evt) => _locker.Lock(async () =>
        {
            switch (evt)
            {
                case ReviewOrderChangedEvent orderChanged:
                    await HandleOrderChanged(orderChanged);
                    break;
                case ComposerStreamChangedEvent streamChanged:
                    await HandleStreamChanged(streamChanged);
                    break;
                default:
                    throw new UnreachableException($"Неподдерживаемый тип события: {evt.GetType().Name}");
            }

            _syncVersion++;

            await notificationService.NotifyQueueUpdated(new OrderQueueSnapshot
            {
                SyncVersion = _syncVersion,
                OrderQueueUpdateType = evt.UpdateType,
                Positions = _queueManager.OrderPositionsById
                    .Select(x => x.Value)
                    .ToArray(),
            });

            _queueManager.ClearRemovedOrderPositions();
        });

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

        private static async Task<DateOnly> GetNearestStreamDate(AppDbContext context)
        {
            ComposerStreamQueries composerStreamQueries = new(context);
            ComposerStreamEntity? nearestStream = await composerStreamQueries.FindNearest(CancellationToken.None);

            return nearestStream?.EventDate ?? DateOnly.MinValue;
        }

        private async Task HandleOrderChanged(ReviewOrderChangedEvent evt)
        {
            if (evt.UpdateType == OrderQueueUpdateType.OrderCanceled && evt.PreviousStatus == ReviewOrderStatus.InProgress)
            {
                await using AppDbContext context = await contextFactory.CreateDbContextAsync();
                ReviewOrderQueries reviewOrderQueries = new(context);
                ReviewOrderEntity? lastTakenOrder = await reviewOrderQueries.FindLastTaken(CancellationToken.None);
                if (lastTakenOrder is not null)
                {
                    _queueManager.PriorityManagerState.UpdateFromOrder(lastTakenOrder);
                }
            }

            _queueManager.UpdateOrder(evt.Order, evt.UpdateType);
        }

        private async Task HandleStreamChanged(ComposerStreamChangedEvent evt)
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            ComposerStreamQueries composerStreamQueries = new(context);
            ReviewOrderQueries reviewOrderQueries = new(context);
            ComposerStreamEntity stream = evt.Stream;

            switch (evt.UpdateType)
            {
                case OrderQueueUpdateType.StreamCreated:
                {
                    _queueManager.NearestStreamDate = stream.EventDate;
                    _queueManager.UpdateOrders();
                    break;
                }
                case OrderQueueUpdateType.StreamStarted:
                {
                    List<ReviewOrderEntity> orders = await reviewOrderQueries.GetOrdersToStartStream(stream.Id, CancellationToken.None);
                    _queueManager.NearestStreamDate = stream.EventDate;
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCompleted:
                {
                    List<ReviewOrderEntity> orders = await reviewOrderQueries.GetOrdersToCompleteStream(stream.Id, CancellationToken.None);
                    _queueManager.NearestStreamDate = await GetNearestStreamDate(context);
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCanceled:
                {
                    _queueManager.NearestStreamDate = await GetNearestStreamDate(context);
                    _queueManager.UpdateOrders();
                    break;
                }
                default:
                    throw new UnreachableException($"Неподдерживаемый тип обновления стрима: {evt.UpdateType}");
            }
        }
    }
}