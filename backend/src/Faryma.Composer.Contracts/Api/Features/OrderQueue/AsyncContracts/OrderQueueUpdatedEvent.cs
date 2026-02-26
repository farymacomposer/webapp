using Faryma.Composer.Contracts.Api.Features.OrderQueue.Dto;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts
{
    /// <summary>
    /// Событие изменения очереди
    /// </summary>
    public sealed record OrderQueueUpdatedEvent
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Тип обновления очереди
        /// </summary>
        public required OrderQueueUpdateType OrderQueueUpdateType { get; init; }

        /// <summary>
        /// Позиции заказов
        /// </summary>
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }

        public static OrderQueueUpdatedEvent Map(OrderQueueSnapshot snapshot)
        {
            return new()
            {
                SyncVersion = snapshot.SyncVersion,
                OrderQueueUpdateType = snapshot.OrderQueueUpdateType,
                OrderPositions = snapshot.Positions.Select(OrderPositionDto.Map),
            };
        }
    }
}