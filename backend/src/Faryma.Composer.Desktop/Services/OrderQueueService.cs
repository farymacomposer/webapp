using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Api.OrderQueue;
using Faryma.Composer.Desktop.Api.OrderQueue.Dto;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Desktop.ViewModels;
using Microsoft.UI.Dispatching;

namespace Faryma.Composer.Desktop.Services
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
            _orderQueueHub.ReceiveSnapshot(message => _dispatcherQueue.EnqueueAsync(() => ReceiveSnapshot(message)));
            _orderQueueHub.ReceiveUpdated(@event => _dispatcherQueue.EnqueueAsync(() => ReceiveUpdated(@event)));
            await _orderQueueHub.Start();
        }

        public Task UpdateOrderQueue() => _orderQueueHub.GetSnapshot();

        private void ReceiveSnapshot(OrderQueueSnapshotMessage message)
        {
            SyncVersion = message.SyncVersion;

            if (message.InProgressOrder is not null)
            {
                InProgressOrder = new ReviewOrderVM(message.InProgressOrder.Order, message.InProgressOrder.CurrentPosition);
            }

            Update(ActiveOrders, message.ActiveOrders);
            Update(CompletedOrders, message.CompletedOrders);
            Update(ScheduledOrders, message.ScheduledOrders);
            Update(FrozenOrders, message.FrozenOrders);

            void Update(ObservableCollection<ReviewOrderVM> target, IEnumerable<OrderPositionDto> source)
            {
                target.Clear();

                foreach (OrderPositionDto item in source.OrderBy(x => x.CurrentPosition.QueueIndex))
                {
                    target.Add(new ReviewOrderVM(item.Order, item.CurrentPosition));
                }
            }
        }

        private async Task ReceiveUpdated(OrderQueueUpdatedEvent @event)
        {
            if (@event.SyncVersion - SyncVersion == 1)
            {
                SyncVersion = @event.SyncVersion;

                foreach (OrderPositionDto item in @event.OrderPositions.OrderByDescending(x => x.PreviousPosition.QueueIndex))
                {
                    RemoveOrder(item.Order, item.PreviousPosition);
                }

                foreach (OrderPositionDto item in @event.OrderPositions.OrderBy(x => x.CurrentPosition.QueueIndex))
                {
                    InsertOrder(item.Order, item.CurrentPosition);
                }
            }
            else
            {
                await UpdateOrderQueue();
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
    }
}