using System.Collections.ObjectModel;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Dispatching;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature
{
    public sealed partial class OrderQueueService : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HubConnection _signalrClient;

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

        private HttpClient HttpClient => _httpClientFactory.CreateClient("Faryma.Composer.Api");

        public OrderQueueService(IHttpClientFactory httpClientFactory)
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _httpClientFactory = httpClientFactory;

            _signalrClient = new HubConnectionBuilder()
                .WithUrl($"{App.BaseAddress}/api/OrderQueueNotificationHub")
                .WithAutomaticReconnect()
                .Build();

            _signalrClient.On<OrderPositionsChangedEvent>("OrderPositionsChanged", OnOrderPositionsChanged);
        }

        public async Task Initialize()
        {
            await Task.Delay(2000);
            await _signalrClient.StartAsync();
            await UpdateOrderQueue();
        }

        public async Task UpdateOrderQueue()
        {
            GetOrderQueueResponse response = (await HttpClient.GetFromJsonAsync<GetOrderQueueResponse>("/api/OrderQueue/GetOrderQueue"))!;

            SyncVersion = response.SyncVersion;

            if (response.InProgressOrder is not null)
            {
                InProgressOrder = new ReviewOrderVM(response.InProgressOrder.Order, response.InProgressOrder.CurrentPosition);
            }

            if (ActiveOrders.Count > 0)
            {
                foreach (OrderPositionDto item in response.ActiveOrders)
                {
                    if (ActiveOrders[item.CurrentPosition.QueueIndex].Id != item.Order.Id)
                    {
                        throw new InvalidOperationException("Нарушена очередность");
                    }
                }
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

        private Task OnOrderPositionsChanged(OrderPositionsChangedEvent message)
        {
            return _dispatcherQueue.EnqueueAsync(async () =>
            {
                if (await CheckSyncVersion(message.SyncVersion))
                {
                    foreach (OrderPositionDto item in message.OrderPositions.OrderByDescending(x => x.PreviousPosition.QueueIndex))
                    {
                        RemoveOrder(item.Order, item.PreviousPosition);
                    }

                    foreach (OrderPositionDto item in message.OrderPositions.OrderBy(x => x.CurrentPosition.QueueIndex))
                    {
                        InsertOrder(item.Order, item.CurrentPosition);
                    }
                }
            });
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
                await UpdateOrderQueue();

                return false;
            }
        }
    }
}