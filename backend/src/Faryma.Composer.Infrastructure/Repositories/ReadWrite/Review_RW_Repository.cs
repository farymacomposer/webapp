using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class Review_RW_Repository(AppDbContext context)
    {
        public ReviewEntity Create(
            ReviewOrderEntity inProgressOrder,
            int rating,
            DateTime createdAt)
        {
            return context.Reviews.Add(new ReviewEntity
            {
                ReviewOrder = inProgressOrder,
                RatingValue = rating,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            }).Entity;
        }
    }
}