using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Api.Shared.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Events
{
    /// <summary>
    /// Добавлен новый заказ
    /// </summary>
    public sealed record NewOrderAddedEvent
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
        /// Текущая позиция заказа в очереди
        /// </summary>
        public required OrderQueuePositionDto CurrentPosition { get; init; }

        public static NewOrderAddedEvent Map(int syncVersion, OrderPosition orderPosition)
        {
            return new()
            {
                SyncVersion = syncVersion,
                Order = ReviewOrderDto.Map(orderPosition.Order),
                CurrentPosition = OrderQueuePositionDto.Map(orderPosition.PositionHistory.Current)
            };
        }
    }
}