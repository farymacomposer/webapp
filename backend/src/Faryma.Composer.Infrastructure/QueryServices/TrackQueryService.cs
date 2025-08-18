using Faryma.Composer.Infrastructure.QueryModels;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.QueryServices
{
    public sealed class TrackQueryService(IDbContextFactory<AppDbContext> contextFactory) : IAsyncDisposable
    {
        private readonly AppDbContext _context = contextFactory.CreateDbContext();

        public IQueryable<TrackQueryModel> GetTracks() => _context.Tracks
            .AsNoTracking()
            .Include(x => x.Country)
            .Include(x => x.Artists)
            .Include(x => x.Genres)
            .Include(x => x.Reviews)
            .Include(x => x.UserRatings)
            .Select(x => new TrackQueryModel
            {
                Title = x.Title,
                ReleaseDate = x.ReleaseDate,
                CoverUrl = x.CoverUrl,
                ExtendedGenres = x.ExtendedGenres,
                Tags = x.Tags,
                Country = x.CountryId == null ? null : new TrackCountryQueryModel { Name = x.Country!.Name },
                Artists = x.Artists.Select(a => new TrackArtistQueryModel
                {
                    Name = a.Name,
                }),
                Genres = x.Genres.Select(a => new TrackGenreQueryModel
                {
                    Name = a.Name,
                }),
                Reviews = x.Reviews.Select(a => new ReviewQueryModel
                {
                    Rating = a.Rating,
                }),
                UserRatings = x.UserRatings.Select(a => new UserTrackRatingQueryModel
                {
                    RatingValue = a.RatingValue,
                }),
            });

        public ValueTask DisposeAsync() => _context.DisposeAsync();
    }
}