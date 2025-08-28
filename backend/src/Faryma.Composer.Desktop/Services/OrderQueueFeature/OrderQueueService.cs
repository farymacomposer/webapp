using System.Collections.ObjectModel;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Events;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Dispatching;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature
{
    public sealed partial class OrderQueueService : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly HttpClient _httpClient;
        private readonly HubConnection _signalrClient;

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

        public OrderQueueService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");

            _signalrClient = new HubConnectionBuilder()
                .WithUrl($"{App.BaseAddress}/api/OrderQueueNotificationHub")
                .WithAutomaticReconnect()
                .Build();

            _signalrClient.On<NewOrderAddedEvent>("NewOrderAdded", OnNewOrderAdded);
            _signalrClient.On<OrderPositionChangedEvent>("OrderPositionChanged", OnOrderPositionChanged);
            //_signalrClient.On<OrderPositionsChangedEvent>("OrderPositionsChanged", OnOrderPositionsChanged);
            _signalrClient.On<OrderRemovedEvent>("OrderRemoved", OnOrderRemoved);

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public async Task Initialize()
        {
            await _signalrClient.StartAsync();

            GetOrderQueueResponse? response = await _httpClient.GetFromJsonAsync<GetOrderQueueResponse>("/api/OrderQueue/GetOrderQueue");
            UpdateOrderQueue(response!);
        }

        private void UpdateOrderQueue(GetOrderQueueResponse response)
        {
            if (response.InProgressOrder is not null)
            {
                InProgressOrder = new ReviewOrderVM(response.InProgressOrder.Order, response.InProgressOrder.CurrentPosition);
            }

            Update(response.ActiveOrders, ActiveOrders);
            Update(response.CompletedOrders, CompletedOrders);
            Update(response.ScheduledOrders, ScheduledOrders);
            Update(response.FrozenOrders, FrozenOrders);

            void Update(ICollection<OrderPositionDto> source, ObservableCollection<ReviewOrderVM> target)
            {
                target.Clear();

                foreach (OrderPositionDto item in source.OrderBy(x => x.CurrentPosition.QueueIndex))
                {
                    target.Add(new ReviewOrderVM(item.Order, item.CurrentPosition));
                }
            }
        }

        private Task OnNewOrderAdded(NewOrderAddedEvent message) => _dispatcherQueue.EnqueueAsync(() =>
        {
            if (message.CurrentPosition.ActivityStatus is not (OrderActivityStatus.Scheduled or OrderActivityStatus.Active))
            {
                throw new InvalidOperationException(message.CurrentPosition.ActivityStatus.ToString());
            }

            InsertOrder(message.Order, message.CurrentPosition);
        });

        private Task OnOrderPositionChanged(OrderPositionChangedEvent message) => _dispatcherQueue.EnqueueAsync(() =>
        {
            switch (message.OrderQueueUpdateType)
            {
                case OrderQueueUpdateType.AddTrackUrl:

                    ObservableCollection<ReviewOrderVM> list = GetOrdersList(message.CurrentPosition.ActivityStatus);
                    list[message.CurrentPosition.QueueIndex].Update(message.Order, message.CurrentPosition);

                    break;

                case OrderQueueUpdateType.Up
                    or OrderQueueUpdateType.TakeInProgress
                    or OrderQueueUpdateType.Complete
                    or OrderQueueUpdateType.Freeze
                    or OrderQueueUpdateType.Unfreeze:

                    RemoveOrder(message.PreviousPosition);
                    InsertOrder(message.Order, message.CurrentPosition);

                    break;

                default:
                    throw new InvalidOperationException(message.OrderQueueUpdateType.ToString());
            }
        });

        private Task OnOrderRemoved(OrderRemovedEvent message) => _dispatcherQueue.EnqueueAsync(() =>
        {
            if (message.Order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new InvalidOperationException(message.Order.Status.ToString());
            }

            RemoveOrder(message.PreviousPosition);
        });

        private void InsertOrder(ReviewOrderDto order, OrderQueuePositionDto position)
        {
            if (position.ActivityStatus == OrderActivityStatus.InProgress)
            {
                InProgressOrder = new ReviewOrderVM(order, position);

                return;
            }

            ObservableCollection<ReviewOrderVM> list = GetOrdersList(position.ActivityStatus);
            list.Insert(position.QueueIndex, new ReviewOrderVM(order, position));
        }

        private void RemoveOrder(OrderQueuePositionDto position)
        {
            if (position.ActivityStatus == OrderActivityStatus.InProgress)
            {
                InProgressOrder = null;

                return;
            }

            ObservableCollection<ReviewOrderVM> list = GetOrdersList(position.ActivityStatus);
            list.RemoveAt(position.QueueIndex);
        }

        private ObservableCollection<ReviewOrderVM> GetOrdersList(OrderActivityStatus status)
        {
            return status switch
            {
                OrderActivityStatus.Active => ActiveOrders,
                OrderActivityStatus.Completed => CompletedOrders,
                OrderActivityStatus.Scheduled => ScheduledOrders,
                OrderActivityStatus.Frozen => FrozenOrders,
                _ => throw new InvalidOperationException(status.ToString()),
            };
        }
    }
}