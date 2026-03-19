using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.Dto;
using Faryma.Composer.Desktop.Api.OrderQueue;
using Faryma.Composer.Desktop.ViewModels;
using Microsoft.UI.Dispatching;

namespace Faryma.Composer.Desktop.Services
{
    public sealed partial class OrderQueueService : ObservableObject, IOrderQueueNotificationClient
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
            _orderQueueHub.ReceiveSnapshot(ReceiveSnapshot);
            await _orderQueueHub.Start();
        }

        public Task UpdateOrderQueue() => _orderQueueHub.GetSnapshot();

        public Task ReceiveSnapshot(OrderQueueSnapshotMessage message) => _dispatcherQueue.EnqueueAsync(() =>
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
        });
    }
}