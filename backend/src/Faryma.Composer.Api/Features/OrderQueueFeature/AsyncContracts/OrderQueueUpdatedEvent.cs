using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts
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
        [Required]
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