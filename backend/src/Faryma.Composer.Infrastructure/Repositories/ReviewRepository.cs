using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Create(
            string comment,
            ComposerStream composerStream,
            ReviewOrder reviewOrder,
            int rating,
            string trackUrl)
        {
            DateTime creationDate = DateTime.UtcNow;

            Review review = new()
            {
                Comment = comment,
                CompletedAt = creationDate,
                ComposerStream = composerStream,
                ReviewOrder = reviewOrder,
                Rating = rating,
                UpdatedAt = creationDate,
                TrackUrl = trackUrl
            };

            return context.Reviews.Add(review).Entity;
        }
    }
}