using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Add(Review review)
        {
            return context.Reviews.Add(review).Entity;
        }

        public Review Create(
            string comment,
            ComposerStream composerStream,
            ReviewOrder reviewOrder,
            int rating,
            Track track)
        {
            Review review = new()
            {
                Comment = comment,
                CompletedAt = DateTime.UtcNow,
                ComposerStream = composerStream,
                ReviewOrder = reviewOrder,
                Rating = rating,
                Track = track,
                UpdatedAt = DateTime.UtcNow,
            };

            return context.Reviews.Add(review).Entity;
        }
    }
}