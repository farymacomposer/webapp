using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Infrastructure.Features.ReviewOrder
{
    public sealed class ReviewStore(AppDbContext appDbContext, DateTimeContext dateTimeContext)
    {
        public ReviewEntity CreateReview(
            ReviewOrderEntity inProgressOrder,
            int rating,
            UserEntity createdByUser)
        {
            return appDbContext.Reviews.Add(new ReviewEntity
            {
                ReviewOrder = inProgressOrder,
                RatingValue = rating,
                CreatedAt = dateTimeContext.Now,
                UpdatedAt = dateTimeContext.Now,
                CreatedByUser = createdByUser,
            }).Entity;
        }
    }
}
