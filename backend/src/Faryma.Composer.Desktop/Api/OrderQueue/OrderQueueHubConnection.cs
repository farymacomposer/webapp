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

        public IDisposable OnOrderPositionsChanged(Func<OrderPositionsChangedEvent, Task> handler) => _signalrClient.On("OrderPositionsChanged", handler);
    }
}