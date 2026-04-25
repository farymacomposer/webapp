using System.Text.Json.Serialization;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Desktop.Api.OrderQueue
{
    public sealed class OrderQueueHubConnection : IOrderQueueNotificationServer
    {
        private readonly HubConnection _signalrClient = new HubConnectionBuilder()
            .WithUrl(App.BaseAddress + IOrderQueueNotificationServer.RoutePattern)
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .WithAutomaticReconnect()
            .Build();

        public Task Start() => _signalrClient.StartAsync();
        public Task GetSnapshot() => _signalrClient.SendAsync(nameof(GetSnapshot));
        public IDisposable ReceiveSnapshot(Func<OrderQueueSnapshotMessage, Task> handler) => _signalrClient.On(nameof(IOrderQueueNotificationClient.ReceiveSnapshot), handler);
    }
}
