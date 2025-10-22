using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Api.Dto;
using Faryma.Composer.Desktop.Api.OrderQueue;
using Faryma.Composer.Desktop.Api.OrderQueue.Dto;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Microsoft.UI.Dispatching;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature
{
    public sealed partial class OrderQueueService : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        private readonly OrderQueueHubConnection _orderQueueHub = new();

        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        [ObservableProperty]
        public partial int SyncVersion { get; private set; }

        /// <summary>
        /// Заказ в работе
        /// </summary>
        [ObservableProperty]
        public partial ReviewOrderVM? InProgressOrder { get; private set; }

        /// <summary>
        /// Активные заказы
        /// </summary>
        public ObservableCollection<ReviewOrderVM> ActiveOrders { get; } = [];

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        public ObservableCollection<ReviewOrderVM> CompletedOrders { get; } = [];

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        public ObservableCollection<ReviewOrderVM> ScheduledOrders { get; } = [];

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        public ObservableCollection<ReviewOrderVM> FrozenOrders { get; } = [];

        public async Task Initialize()
        {
#if DEBUG
            await Task.Delay(2000);
#endif

            _orderQueueHub.ReceiveOrderQueueUpdated(@event => _dispatcherQueue.EnqueueAsync(() => ReceiveOrderQueueUpdated(@event)));
            _orderQueueHub.ReceiveOrderQueueSnapshot(message => _dispatcherQueue.EnqueueAsync(() => ReceiveOrderQueueSnapshot(message)));
            await _orderQueueHub.Start();
        }

        private async Task ReceiveOrderQueueSnapshot(OrderQueueSnapshotMessage message)
        {
            SyncVersion = message.SyncVersion;

            if (message.InProgressOrder is not null)
            {
                InProgressOrder = new ReviewOrderVM(message.InProgressOrder.Order, message.InProgressOrder.CurrentPosition);
            }

            if (ActiveOrders.Count > 0)
            {
                foreach (OrderPositionDto item in message.ActiveOrders)
                {
                    if (ActiveOrders[item.CurrentPosition.QueueIndex].Id != item.Order.Id)
                    {
                        throw new InvalidOperationException("Нарушена очередность");
                    }
                }
            }

            Update(message.ActiveOrders, ActiveOrders);
            Update(message.CompletedOrders, CompletedOrders);
            Update(message.ScheduledOrders, ScheduledOrders);
            Update(message.FrozenOrders, FrozenOrders);

            void Update(ICollection<OrderPositionDto> source, ObservableCollection<ReviewOrderVM> target)
            {
                target.Clear();

                foreach (OrderPositionDto item in source.OrderBy(x => x.CurrentPosition.QueueIndex))
                {
                    target.Add(new ReviewOrderVM(item.Order, item.CurrentPosition));
                }
            }
        }

        public Task GetOrderQueueSnapshot() => _orderQueueHub.GetOrderQueueSnapshot();

        private async Task ReceiveOrderQueueUpdated(OrderQueueUpdatedEvent @event)
        {
            if (await CheckSyncVersion(@event.SyncVersion))
            {
                foreach (OrderPositionDto item in @event.OrderPositions.OrderByDescending(x => x.PreviousPosition.QueueIndex))
                {
                    RemoveOrder(item.Order, item.PreviousPosition);
                }

                foreach (OrderPositionDto item in @event.OrderPositions.OrderBy(x => x.CurrentPosition.QueueIndex))
                {
                    InsertOrder(item.Order, item.CurrentPosition);
                }
            }
        }

        private void RemoveOrder(ReviewOrderDto order, OrderQueuePositionDto position)
        {
            if (position.ActivityStatus == OrderActivityStatus.Unspecified)
            {
                return;
            }

            if (position.ActivityStatus == OrderActivityStatus.InProgress)
            {
                InProgressOrder = null;

                return;
            }

            ObservableCollection<ReviewOrderVM> list = GetOrdersList(position.ActivityStatus);

            if (list[position.QueueIndex].Id != order.Id)
            {
                throw new InvalidOperationException("Нарушена очередность");
            }

            list.RemoveAt(position.QueueIndex);
        }

        private void InsertOrder(ReviewOrderDto order, OrderQueuePositionDto position)
        {
            if (position.ActivityStatus == OrderActivityStatus.Removed)
            {
                return;
            }

            if (position.ActivityStatus == OrderActivityStatus.InProgress)
            {
                InProgressOrder = new ReviewOrderVM(order, position);

                return;
            }

            ObservableCollection<ReviewOrderVM> list = GetOrdersList(position.ActivityStatus);
            list.Insert(position.QueueIndex, new ReviewOrderVM(order, position));
        }

        private ObservableCollection<ReviewOrderVM> GetOrdersList(OrderActivityStatus status)
        {
            return status switch
            {
                OrderActivityStatus.Active => ActiveOrders,
                OrderActivityStatus.Completed => CompletedOrders,
                OrderActivityStatus.Scheduled => ScheduledOrders,
                OrderActivityStatus.Frozen => FrozenOrders,
                _ => throw new InvalidOperationException($"Статус активности заказа '{status}' не поддерживается"),
            };
        }

        private async Task<bool> CheckSyncVersion(int newSyncVersion)
        {
            if (newSyncVersion - SyncVersion == 1)
            {
                SyncVersion = newSyncVersion;

                return true;
            }
            else
            {
                await GetOrderQueueSnapshot();

                return false;
            }
        }
    }
}