using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.GetOrderQueue
{
    /// <summary>
    /// Ответ на запрос получения очереди заказов
    /// </summary>
    public sealed record GetOrderQueueResponse
    {
        /// <summary>
        /// Хэш-код позиций заказов
        /// </summary>
        public required int PositionsHashCode { get; init; }

        /// <summary>
        /// Активные заказы
        /// </summary>
        [Required]
        public ICollection<OrderPositionDto> ActiveOrders { get; } = [];

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public OrderPositionDto? InProgressOrder { get; set; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        [Required]
        public ICollection<OrderPositionDto> CompletedOrders { get; } = [];

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        [Required]
        public ICollection<OrderPositionDto> ScheduledOrders { get; } = [];

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        [Required]
        public ICollection<OrderPositionDto> FrozenOrders { get; } = [];

        public static GetOrderQueueResponse Map(OrderQueue orderQueue)
        {
            GetOrderQueueResponse result = new()
            {
                PositionsHashCode = orderQueue.PositionsHashCode,
            };

            foreach (OrderPosition position in orderQueue.Positions.OrderBy(x => x.PositionHistory.Current.QueueIndex))
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