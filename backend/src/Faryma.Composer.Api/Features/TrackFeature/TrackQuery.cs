using Faryma.Composer.Contracts.QueryModels;
using Faryma.Composer.Contracts.QueryServices;
using HotChocolate.Data;

namespace Faryma.Composer.Api.Features.TrackFeature
{
    public sealed class TrackQuery
    {
        [UsePaging(MaxPageSize = 50)]
        [UseFiltering(typeof(TrackQueryModelFilterInput))]
        [UseSorting(typeof(TrackQueryModelSortInput))]
        public IQueryable<TrackQueryModel> GetTracks([Service] TrackQueryService service) => service.GetTracks();
    }
}