using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.GetOrderQueue
{
    /// <summary>
    /// Ответ на запрос получения очереди заказов
    /// </summary>
    public sealed record GetOrderQueueResponse
    {
        /// <summary>
        /// Активные заказы
        /// </summary>
        public ICollection<OrderPositionDto> ActiveOrders { get; } = [];

        /// <summary>
        /// Заказ в работе
        /// </summary>
        public OrderPositionDto? InProgressOrder { get; set; }

        /// <summary>
        /// Выполненные заказы
        /// </summary>
        public ICollection<OrderPositionDto> CompletedOrders { get; } = [];

        /// <summary>
        /// Запланированные заказы
        /// </summary>
        public ICollection<OrderPositionDto> ScheduledOrders { get; } = [];

        /// <summary>
        /// Замороженные заказы
        /// </summary>
        public ICollection<OrderPositionDto> FrozenOrders { get; } = [];

        public static GetOrderQueueResponse Map(Dictionary<long, OrderPosition> orders)
        {
            GetOrderQueueResponse result = new();

            foreach (KeyValuePair<long, OrderPosition> kvp in orders.OrderBy(x => x.Value.PositionHistory.Current.QueueIndex))
            {
                switch (kvp.Value.PositionHistory.Current.ActivityStatus)
                {
                    case Core.Features.OrderQueueFeature.Enums.OrderActivityStatus.Active:
                        result.ActiveOrders.Add(OrderPositionDto.Map(kvp.Value));
                        break;

                    case Core.Features.OrderQueueFeature.Enums.OrderActivityStatus.InProgress:
                        result.InProgressOrder = OrderPositionDto.Map(kvp.Value);
                        break;

                    case Core.Features.OrderQueueFeature.Enums.OrderActivityStatus.Completed:
                        result.CompletedOrders.Add(OrderPositionDto.Map(kvp.Value));
                        break;

                    case Core.Features.OrderQueueFeature.Enums.OrderActivityStatus.Scheduled:
                        result.ScheduledOrders.Add(OrderPositionDto.Map(kvp.Value));
                        break;

                    case Core.Features.OrderQueueFeature.Enums.OrderActivityStatus.Frozen:
                        result.FrozenOrders.Add(OrderPositionDto.Map(kvp.Value));
                        break;
                }
            }

            return result;
        }
    }
}