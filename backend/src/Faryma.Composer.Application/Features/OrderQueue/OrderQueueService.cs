using Faryma.Composer.Application.Common;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed partial class OrderQueueService(
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
            UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            DateOnly nearestStreamDate = await GetNearestStreamDate(uow);
            ReviewOrderEntity? lastTakenOrder = await FindLastTaken(uow);
            ReviewOrderEntity? lastTakenDebt = await FindLastTakenDebt(uow);
            ReviewOrderEntity? lastTakenOutOfQueue = await FindLastTakenOutOfQueue(uow);
            Dictionary<DateOnly, string> lastNicknamesByStreamDate = await GetLastNicknamesByStreamDate(uow);
            List<ReviewOrderEntity> orders = await GetOrdersInQueue(uow);

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
                    await HandleOrderChanged(orderChanged);
                    break;
                case ComposerStreamChangedEvent streamChanged:
                    await HandleStreamChanged(streamChanged);
                    break;
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
                UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                ReviewOrderEntity? lastTakenOrder = await FindLastTaken(uow);
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
            UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

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
                    List<ReviewOrderEntity> orders = await GetOrdersToStartStream(uow, stream.Id);
                    _queueManager.NearestStreamDate = stream.EventDate;
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCompleted:
                {
                    List<ReviewOrderEntity> orders = await GetOrdersToCompleteStream(uow, stream.Id);
                    _queueManager.NearestStreamDate = await GetNearestStreamDate(uow);
                    _queueManager.UpdateOrders(orders);
                    break;
                }
                case OrderQueueUpdateType.StreamCanceled:
                {
                    _queueManager.NearestStreamDate = await GetNearestStreamDate(uow);
                    _queueManager.UpdateOrders();
                    break;
                }
                default:
                    throw new InvalidOperationException($"Неподдерживаемый тип обновления стрима: {evt.UpdateType}");
            }
        }
    }
}
