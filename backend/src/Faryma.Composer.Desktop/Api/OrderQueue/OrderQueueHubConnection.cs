using System.Text.Json.Serialization;
using Faryma.Composer.Desktop.Api.OrderQueue.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Desktop.Api.OrderQueue
{
    public sealed class OrderQueueHubConnection
    {
        private readonly HubConnection _signalrClient = new HubConnectionBuilder()
            .WithUrl($"{App.BaseAddress}/api/OrderQueueNotificationHub")
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .WithAutomaticReconnect()
            .Build();

        public Task Start() => _signalrClient.StartAsync();
        public Task GetSnapshot() => _signalrClient.SendAsync(nameof(GetSnapshot));
        public IDisposable ReceiveSnapshot(Func<OrderQueueSnapshotMessage, Task> handler) => _signalrClient.On(nameof(ReceiveSnapshot), handler);
        public IDisposable ReceiveUpdated(Func<OrderQueueUpdatedEvent, Task> handler) => _signalrClient.On(nameof(ReceiveUpdated), handler);
    }
}