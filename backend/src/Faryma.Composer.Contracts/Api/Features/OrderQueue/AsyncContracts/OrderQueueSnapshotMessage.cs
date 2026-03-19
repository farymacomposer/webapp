using System.Diagnostics;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.Dto;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;

namespace Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts
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
        public required List<OrderPositionDto> ActiveOrders { get; init; }

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public OrderPositionDto? InProgressOrder { get; private set; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        public required List<OrderPositionDto> CompletedOrders { get; init; }

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        public required List<OrderPositionDto> ScheduledOrders { get; init; }

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        public required List<OrderPositionDto> FrozenOrders { get; init; }

        public static OrderQueueSnapshotMessage Map(OrderQueueSnapshot snapshot)
        {
            OrderQueueSnapshotMessage result = new()
            {
                SyncVersion = snapshot.SyncVersion,
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
                        throw new UnreachableException($"Неподдерживаемый статус активности заказа '{position.PositionHistory.Current.ActivityStatus}'");
                }
            }

            return result;
        }
    }
}