using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Infrastructure.Features.ReviewOrder
{
    public sealed class ReviewStore(AppDbContext appDbContext, DateTimeService dateTimeService)
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
                CreatedAt = dateTimeService.Now,
                UpdatedAt = dateTimeService.Now,
                CreatedByUser = createdByUser,
            }).Entity;
        }
    }
}
