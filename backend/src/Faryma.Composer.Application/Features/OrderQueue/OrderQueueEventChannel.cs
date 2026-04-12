using System.Threading.Channels;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueEventChannel
    {
        private readonly Channel<OrderQueueEvent> _channel = Channel.CreateUnbounded<OrderQueueEvent>(new()
        {
            SingleReader = true,
            SingleWriter = false,
        });

        public void Write(OrderQueueEvent item) => _channel.Writer.TryWrite(item);
        public bool TryRead(out OrderQueueEvent? item) => _channel.Reader.TryRead(out item);
        public IAsyncEnumerable<OrderQueueEvent> ReadAll(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }
}