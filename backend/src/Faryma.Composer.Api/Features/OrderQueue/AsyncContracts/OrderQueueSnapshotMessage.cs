using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.OrderQueue.Dto;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Api.Features.OrderQueue.AsyncContracts
{
    /// <summary>
    /// Сообщение о состоянии очереди
    /// </summary>
    public sealed record OrderQueueSnapshotMessage
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
        /// Активные заказы
        /// </summary>
        [Required]
        public required List<OrderPositionDto> ActiveOrders { get; init; }

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public OrderPositionDto? InProgressOrder { get; set; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        [Required]
        public required List<OrderPositionDto> CompletedOrders { get; init; }

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        [Required]
        public required List<OrderPositionDto> ScheduledOrders { get; init; }

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        [Required]
        public required List<OrderPositionDto> FrozenOrders { get; init; }

        public static OrderQueueSnapshotMessage Map(OrderQueueSnapshot snapshot)
        {
            OrderQueueSnapshotMessage result = new()
            {
                SyncVersion = snapshot.SyncVersion,
                OrderQueueUpdateType = snapshot.OrderQueueUpdateType,
                ActiveOrders = new List<OrderPositionDto>(),
                CompletedOrders = new List<OrderPositionDto>(),
                ScheduledOrders = new List<OrderPositionDto>(),
                FrozenOrders = new List<OrderPositionDto>(),
            };

            foreach (OrderPosition position in snapshot.Positions.OrderBy(x => x.PositionHistory.Current.QueueIndex))
            {
                OrderPositionDto dto = OrderPositionDto.Map(position);

                switch (position.PositionHistory.Current.ActivityStatus)
                {
                    case OrderActivityStatus.Active:
                    {
                        result.ActiveOrders.Add(dto);
                        break;
                    }
                    case OrderActivityStatus.InProgress:
                    {
                        result.InProgressOrder = dto;
                        break;
                    }
                    case OrderActivityStatus.Completed:
                    {
                        result.CompletedOrders.Add(dto);
                        break;
                    }
                    case OrderActivityStatus.Scheduled:
                    {
                        result.ScheduledOrders.Add(dto);
                        break;
                    }
                    case OrderActivityStatus.Frozen:
                    {
                        result.FrozenOrders.Add(dto);
                        break;
                    }
                    case OrderActivityStatus.Removed:
                        break;
                    default:
                        throw new InvalidOperationException($"Неподдерживаемый статус активности заказа '{position.PositionHistory.Current.ActivityStatus}'");
                }
            }

            return result;
        }
    }
}
