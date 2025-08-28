using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Events;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.SignalR.Client;

namespace Faryma.Composer.Desktop.Services.OrderQueueFeature
{
    public sealed partial class OrderQueueService : ObservableObject
    {
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
                .WithUrl("address/monitoring-hub")
                .WithAutomaticReconnect()
                .Build();

            _signalrClient.On<NewOrderAddedEvent>("NewOrderAdded", OnNewOrderAdded);
            _signalrClient.On<OrderPositionChangedEvent>("OrderPositionChanged", OnOrderPositionChanged);
            //_signalrClient.On<OrderPositionsChangedEvent>("OrderPositionsChanged", OnOrderPositionsChanged);
            _signalrClient.On<OrderRemovedEvent>("OrderRemoved", OnOrderRemoved);
        }

        public void OnNewOrderAdded(NewOrderAddedEvent message)
        {
            if (message.CurrentPosition.ActivityStatus is not (OrderActivityStatus.Scheduled or OrderActivityStatus.Active))
            {
                throw new InvalidOperationException(message.CurrentPosition.ActivityStatus.ToString());
            }

            InsertOrder(message.Order, message.CurrentPosition);
        }

        public void OnOrderPositionChanged(OrderPositionChangedEvent message)
        {
            switch (message.OrderQueueUpdateType)
            {
                case OrderQueueUpdateType.AddTrackUrl:

                    ObservableCollection<ReviewOrderVM> list = GetOrdersList(message.CurrentPosition.ActivityStatus);
                    list[message.CurrentPosition.QueueIndex].Update(message.Order);

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
        }

        public void OnOrderRemoved(OrderRemovedEvent message)
        {
            if (message.Order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new InvalidOperationException(message.Order.Status.ToString());
            }

            RemoveOrder(message.PreviousPosition);
        }

        private void InsertOrder(ReviewOrderDto order, OrderQueuePositionDto position)
        {
            if (position.ActivityStatus == OrderActivityStatus.InProgress)
            {
                InProgressOrder = new ReviewOrderVM(order);

                return;
            }

            ObservableCollection<ReviewOrderVM> list = GetOrdersList(position.ActivityStatus);
            list.Insert(position.QueueIndex, new ReviewOrderVM(order));
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