using Faryma.Composer.Api.Features.CommonDto;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Изменена позиция заказа
    /// </summary>
    public sealed record OrderPositionChangedEvent
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
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto Order { get; init; }

        /// <summary>
        /// Предыдущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto PreviousPosition { get; init; }

        /// <summary>
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }

        public static OrderPositionChangedEvent Map(int syncVersion, OrderPosition orderPosition, OrderQueueUpdateType updateType)
        {
            return new()
            {
                SyncVersion = syncVersion,
                OrderQueueUpdateType = updateType,
                Order = ReviewOrderDto.Map(orderPosition.Order),
                PreviousPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Previous),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };
        }
    }
}