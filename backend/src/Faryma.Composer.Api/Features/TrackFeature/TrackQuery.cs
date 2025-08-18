using Faryma.Composer.Infrastructure.QueryModels;
using Faryma.Composer.Infrastructure.QueryServices;

namespace Faryma.Composer.Api.Features.TrackFeature
{
    public sealed class TrackQuery
    {
        [UsePaging(MaxPageSize = 50)]
        [UseFiltering]
        [UseSorting]
        public IQueryable<TrackQueryModel> GetTracks([Service] TrackQueryService service) => service.GetTracks();
    }
}