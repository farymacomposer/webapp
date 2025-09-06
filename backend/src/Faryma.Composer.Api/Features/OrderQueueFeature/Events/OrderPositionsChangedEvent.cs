using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Изменены позиции заказов
    /// </summary>
    public sealed record OrderPositionsChangedEvent
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
        [Required]
        public required IEnumerable<OrderPositionDto> OrderPositions { get; init; }

        public static OrderPositionsChangedEvent Map(OrderQueue orderQueue)
        {
            return new()
            {
                SyncVersion = orderQueue.SyncVersion,
                OrderQueueUpdateType = orderQueue.OrderQueueUpdateType,
                OrderPositions = orderQueue.Positions.Select(OrderPositionDto.Map),
            };
        }
    }
}