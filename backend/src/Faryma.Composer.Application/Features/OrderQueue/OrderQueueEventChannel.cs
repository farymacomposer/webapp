using System.Threading.Channels;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

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
        public IAsyncEnumerable<OrderQueueEvent> ReadAll(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }

    /// <summary>
    /// Событие обновления очереди
    /// </summary>
    public abstract class OrderQueueEvent(OrderQueueUpdateType updateType)
    {
        /// <summary>
        /// Тип обновления очереди
        /// </summary>
        public OrderQueueUpdateType UpdateType { get; } = updateType;
    }

    /// <summary>
    /// Заказ был обновлен
    /// </summary>
    public sealed class OrderUpdatedEvent(ReviewOrderEntity order, OrderQueueUpdateType updateType) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity Order { get; } = order;
    }

    /// <summary>
    /// Заказ был отменен
    /// </summary>
    public sealed class OrderCanceledEvent(ReviewOrderEntity order, ReviewOrderStatus previousStatus) : OrderQueueEvent(OrderQueueUpdateType.OrderCanceled)
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity Order { get; } = order;

        /// <summary>
        /// Статус заказа перед отменой
        /// </summary>
        public ReviewOrderStatus PreviousStatus { get; } = previousStatus;
    }

    /// <summary>
    /// Был создан стрим
    /// </summary>
    public sealed class StreamCreatedEvent(ComposerStreamEntity stream, OrderQueueUpdateType updateType) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public ComposerStreamEntity Stream { get; } = stream;
    }

    /// <summary>
    /// Стрим был отменен
    /// </summary>
    public sealed class StreamCanceledEvent() : OrderQueueEvent(OrderQueueUpdateType.StreamCanceled)
    {
    }
}