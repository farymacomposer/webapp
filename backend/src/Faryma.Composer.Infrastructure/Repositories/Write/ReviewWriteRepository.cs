using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class ReviewWriteRepository(AppDbContext context)
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