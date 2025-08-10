using Faryma.Composer.Core.Features.TrackFeature;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Api.Features.TrackFeature
{
    public class TrackQuery()
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<TrackDto> GetTracks([Service] TrackService trackService) =>
            trackService.GetAll().Select(t => TrackDto.Map(t));

        public TrackDto GetTrackById(
            [Service] TrackService trackService,
            [ID] int id)
        {
            try
            {
                Track track = trackService.Find(id);
                return TrackDto.Map(track);
            }
            catch (InvalidOperationException ex)
            {
                throw new GraphQLException(new Error(ex.Message));
            }
        }
    }
}