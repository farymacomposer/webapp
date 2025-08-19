using Faryma.Composer.Infrastructure.QueryModels;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.QueryServices
{
    public sealed class TrackQueryService(IDbContextFactory<AppDbContext> contextFactory) : IAsyncDisposable
    {
        private readonly AppDbContext _context = contextFactory.CreateDbContext();

        public IQueryable<TrackQueryModel> GetTracks() => _context.Tracks
            .AsNoTracking()
            .Select(x => new TrackQueryModel
            {
                Title = x.Title,
                ReleaseYear = (x.ReleaseDate != null) ? x.ReleaseDate.Value.Year : null,
                CountryId = x.CountryId,
                CoverUrl = x.CoverUrl,
                HasReview = x.Reviews.Count > 0,
                LastReviewRating = (x.Reviews.Count > 0) ? x.Reviews.OrderBy(x => x.CreatedAt).Last().RatingValue : null,
                UserRating = (x.UserRatings.Count > 0) ? x.UserRatings.Sum(x => x.RatingValue) : null,
                ExtendedGenres = x.ExtendedGenres,
                Tags = x.Tags,
                Artists = x.Artists.Select(x => x.Name),
                GenreIds = x.Genres.Select(x => x.Id),
            });

        public ValueTask DisposeAsync() => _context.DisposeAsync();
    }
}