using Faryma.Composer.Application.Common;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure.Features.OrderQueue;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueService(
        IOrderQueueNotificationService notificationService,
        IServiceScopeFactory scopeFactory)
    {
        private readonly SemaphoreLocker _locker = new();
        private OrderQueueManager _queueManager = null!;

        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        private int _syncVersion;

        public async Task Initialize()
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            OrderQueueQueries orderQueueQueries = scope.ServiceProvider.GetRequiredService<OrderQueueQueries>();

            DateOnly nearestStreamDate = await orderQueueQueries.GetNearestStreamDate();
            ReviewOrderEntity? lastTakenOrder = await orderQueueQueries.FindLastTaken();
            ReviewOrderEntity? lastTakenDebt = await orderQueueQueries.FindLastTakenDebt();
            ReviewOrderEntity? lastTakenOutOfQueue = await orderQueueQueries.FindLastTakenOutOfQueue();
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await orderQueueQueries.GetLastNicknamesByStreamDate();
            List<ReviewOrderEntity> orders = await orderQueueQueries.GetOrdersInQueue();

            _queueManager = new OrderQueueManager
            {
                NearestStreamDate = nearestStreamDate,
                OrderPositionsById = orders.ToDictionary(k => k.Id, OrderPosition.Create),
                PriorityManagerState = new OrderPriorityManagerState
                {
                    LastPriorityManagerState = (lastTakenOrder is null)
                        ? CategoryState.Initial
                        : OrderPriorityManagerState.MapCategoryState(lastTakenOrder.QueueCategory),
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
                {
                    await HandleOrderChanged(orderChanged);
                    break;
                }
                case ComposerStreamChangedEvent streamChanged:
                {
                    await HandleStreamChanged(streamChanged);
                    break;
                }
                default:
                    throw new InvalidOperationException($"Неподдерживаемый тип события: {evt.GetType().Name}");
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

        private async Task HandleOrderChanged(ReviewOrderChangedEvent evt)
        {
            if (evt.UpdateType == OrderQueueUpdateType.OrderCanceled && evt.PreviousStatus == ReviewOrderStatus.InProgress)
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                OrderQueueQueries orderQueueQueries = scope.ServiceProvider.GetRequiredService<OrderQueueQueries>();

                ReviewOrderEntity? lastTakenOrder = await orderQueueQueries.FindLastTaken();
                if (lastTakenOrder is not null)
                {
                    _queueManager.PriorityManagerState.UpdateFromOrder(lastTakenOrder);
                }
            }

            _queueManager.UpdateOrder(evt.Order, evt.UpdateType);
        }

        private async Task HandleStreamChanged(ComposerStreamChangedEvent evt)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            OrderQueueQueries orderQueueQueries = scope.ServiceProvider.GetRequiredService<OrderQueueQueries>();

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
                    List<ReviewOrderEntity> orders = await orderQueueQueries.GetOrdersToStartStream(stream.Id);
                    _queueManager.NearestStreamDate = stream.EventDate;
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCompleted:
                {
                    List<ReviewOrderEntity> orders = await orderQueueQueries.GetOrdersToCompleteStream(stream.Id);
                    _queueManager.NearestStreamDate = await orderQueueQueries.GetNearestStreamDate();
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCanceled:
                {
                    _queueManager.NearestStreamDate = await orderQueueQueries.GetNearestStreamDate();
                    _queueManager.UpdateOrders();
                    break;
                }
                default:
                    throw new InvalidOperationException($"Неподдерживаемый тип обновления стрима: {evt.UpdateType}");
            }
        }
    }
}
