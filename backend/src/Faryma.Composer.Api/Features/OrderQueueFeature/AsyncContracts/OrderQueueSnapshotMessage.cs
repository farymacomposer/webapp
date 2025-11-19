using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.AsyncContracts
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
        /// Активные заказы
        /// </summary>
        [Required]
        public List<OrderPositionDto> ActiveOrders { get; } = [];

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public OrderPositionDto? InProgressOrder { get; private set; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        [Required]
        public List<OrderPositionDto> CompletedOrders { get; } = [];

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        [Required]
        public List<OrderPositionDto> ScheduledOrders { get; } = [];

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        [Required]
        public List<OrderPositionDto> FrozenOrders { get; } = [];

        public static OrderQueueSnapshotMessage Map(OrderQueueSnapshot snapshot)
        {
            OrderQueueSnapshotMessage result = new()
            {
                SyncVersion = snapshot.SyncVersion,
            };

            foreach (OrderPosition position in snapshot.Positions.OrderBy(x => x.PositionHistory.Current.QueueIndex))
            {
                OrderPositionDto dto = OrderPositionDto.Map(position);

                switch (position.PositionHistory.Current.ActivityStatus)
                {
                    case OrderActivityStatus.Active:
                        result.ActiveOrders.Add(dto);
                        break;

                    case OrderActivityStatus.InProgress:
                        result.InProgressOrder = dto;
                        break;

                    case OrderActivityStatus.Completed:
                        result.CompletedOrders.Add(dto);
                        break;

                    case OrderActivityStatus.Scheduled:
                        result.ScheduledOrders.Add(dto);
                        break;

                    case OrderActivityStatus.Frozen:
                        result.FrozenOrders.Add(dto);
                        break;

                    default:
                        throw new OrderQueueException($"Статус активности заказа '{position.PositionHistory.Current.ActivityStatus}' не поддерживается");
                }
            }

            return result;
        }
    }
}