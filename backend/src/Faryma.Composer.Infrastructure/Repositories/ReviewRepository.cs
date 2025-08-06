using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Create(
            ReviewOrder inProgressOrder,
            ComposerStream liveStream,
            int rating,
            string comment)
        {
            DateTime now = DateTime.UtcNow;

            return context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                ComposerStream = liveStream,
                Rating = rating,
                Comment = comment,
                CompletedAt = now,
                UpdatedAt = now,
            }).Entity;
        }
    }
}