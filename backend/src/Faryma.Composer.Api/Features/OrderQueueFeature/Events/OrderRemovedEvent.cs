using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Api.Shared.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Заказ удален
    /// </summary>
    public sealed record OrderRemovedEvent
    {
        /// <summary>
        /// Версия для синхронизации состояния очереди
        /// </summary>
        public required int SyncVersion { get; init; }

        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        public static OrderRemovedEvent Map(int syncVersion, OrderPosition orderPosition)
        {
            return new()
            {
                SyncVersion = syncVersion,
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous)
            };
        }
    }
}