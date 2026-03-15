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
    /// Заказ был изменен
    /// </summary>
    public sealed class ReviewOrderChangedEvent(ReviewOrderEntity order, OrderQueueUpdateType updateType, ReviewOrderStatus previousStatus) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public ReviewOrderEntity Order { get; } = order;

        /// <summary>
        /// Предыдущий статус заказа
        /// </summary>
        public ReviewOrderStatus PreviousStatus { get; } = previousStatus;
    }

    /// <summary>
    /// Стрим композитора был изменен
    /// </summary>
    public sealed class ComposerStreamChangedEvent(ComposerStreamEntity stream, OrderQueueUpdateType updateType) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public ComposerStreamEntity Stream { get; } = stream;
    }
}