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
        public Task GetSnapshot() => _signalrClient.SendAsync(nameof(GetSnapshot));
        public IDisposable ReceiveSnapshot(Func<OrderQueueSnapshotMessage, Task> handler) => _signalrClient.On(nameof(ReceiveSnapshot), handler);
        public IDisposable ReceiveUpdated(Func<OrderQueueUpdatedEvent, Task> handler) => _signalrClient.On(nameof(ReceiveUpdated), handler);
    }
}