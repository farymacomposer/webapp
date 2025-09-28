using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class Review_RW_Repository(AppDbContext context)
    {
        public Review Create(
            ReviewOrder inProgressOrder,
            int rating,
            DateTime createdAt)
        {
            return context.Reviews.Add(new Review
            {
                ReviewOrder = inProgressOrder,
                RatingValue = rating,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            }).Entity;
        }
    }
}