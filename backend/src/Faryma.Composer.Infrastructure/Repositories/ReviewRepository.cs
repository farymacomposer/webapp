using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Create(
            ReviewOrder inProgressOrder,
            int rating,
            DateTime updatedAt)
        {
            return context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                Rating = rating,
                UpdatedAt = updatedAt,
            }).Entity;
        }
    }
}