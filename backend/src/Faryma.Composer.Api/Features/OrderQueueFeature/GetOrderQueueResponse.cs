using Faryma.Composer.Api.Features.OrderQueueFeature.Dto;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed record GetOrderQueueResponse
    {
        public required IEnumerable<OrderQueueItemDto> Items { get; set; }

        public static GetOrderQueueResponse Map(IReadOnlyDictionary<long, OrderPosition> orders)
        {
            return new()
            {
                Items = orders.Select(x => OrderQueueItemDto.Map(x.Value))
            };
        }
    }
}