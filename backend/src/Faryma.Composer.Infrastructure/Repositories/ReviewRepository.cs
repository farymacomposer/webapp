using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Create(
            ReviewOrder inProgressOrder,
            int rating,
            string comment)
        {
            DateTime now = DateTime.UtcNow;

            return context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                TrackUrl = inProgressOrder.TrackUrl!,
                Rating = rating,
                Comment = comment,
                UpdatedAt = now,
            }).Entity;
        }
    }
}