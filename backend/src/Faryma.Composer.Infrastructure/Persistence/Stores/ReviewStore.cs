using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ReviewStore(AppDbContext context)
    {
        public ReviewEntity Create(
            ReviewOrderEntity inProgressOrder,
            int rating,
            DateTime createdAt)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rating);

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