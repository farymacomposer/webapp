using Faryma.Composer.Core.Features.OrderQueueFeature.Models;

namespace Faryma.Composer.Api.Features.OrderQueueFeature
{
    public sealed record GetOrderQueueResponse
    {
        public static GetOrderQueueResponse Map(IReadOnlyDictionary<long, OrderPosition> orders) => new();
    }
}