using System.Threading.Channels;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueEventChannel
    {
        private readonly Channel<OrderQueueEvent> _channel = Channel.CreateUnbounded<OrderQueueEvent>(new()
        {
            SingleReader = true,
            SingleWriter = false,
        });

        public void Write(ComposerStreamEntity stream, OrderQueueUpdateType updateType) =>
            _channel.Writer.TryWrite(new ComposerStreamChangedEvent(stream, updateType));

        public void Write(ReviewOrderEntity order, OrderQueueUpdateType updateType, ReviewOrderStatus previousStatus) =>
            _channel.Writer.TryWrite(new ReviewOrderChangedEvent(order, updateType, previousStatus));

        public bool TryRead(out OrderQueueEvent? item) => _channel.Reader.TryRead(out item);
        public IAsyncEnumerable<OrderQueueEvent> ReadAll(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }
}
