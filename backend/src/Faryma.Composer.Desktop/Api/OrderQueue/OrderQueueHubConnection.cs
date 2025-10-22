using Faryma.Composer.Desktop.Api.OrderQueue.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace Faryma.Composer.Desktop.Api.OrderQueue
{
    public sealed class OrderQueueHubConnection
    {
        private readonly HubConnection _signalrClient;

        public OrderQueueHubConnection()
        {
            _signalrClient = new HubConnectionBuilder()
                .WithUrl($"{App.BaseAddress}/api/OrderQueueNotificationHub")
                .WithAutomaticReconnect()
                .Build();
        }

        public Task Start() => _signalrClient.StartAsync();
        public Task GetOrderQueueSnapshot() => _signalrClient.SendAsync(nameof(GetOrderQueueSnapshot));
        public IDisposable ReceiveOrderQueueSnapshot(Func<OrderQueueSnapshotMessage, Task> handler) => _signalrClient.On(nameof(ReceiveOrderQueueSnapshot), handler);
        public IDisposable ReceiveOrderQueueUpdated(Func<OrderQueueUpdatedEvent, Task> handler) => _signalrClient.On(nameof(ReceiveOrderQueueUpdated), handler);
    }
}