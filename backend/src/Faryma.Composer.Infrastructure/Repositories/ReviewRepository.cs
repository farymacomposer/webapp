using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Create(
            ReviewOrder inProgressOrder,
            int rating,
            string comment,
            DateTime updatedAt)
        {
            return context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                Rating = rating,
                Comment = comment,
                UpdatedAt = updatedAt,
            }).Entity;
        }
    }
}