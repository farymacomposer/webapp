using Faryma.Composer.Core.Features.TrackFeature;

namespace Faryma.Composer.Api.Features.TrackFeature
{
    public sealed class TrackQuery()
    {
        [UsePaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<TrackDto> GetTracks([Service] TrackService trackService) => trackService
            .GetAll()
            .Select(x => TrackDto.Map(x));
    }
}