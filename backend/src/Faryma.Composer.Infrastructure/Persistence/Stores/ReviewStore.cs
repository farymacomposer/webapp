using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ReviewStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public ReviewEntity Create(
            ReviewOrderEntity inProgressOrder,
            int rating,
            UserEntity createdByUser)
        {
            return context.Reviews.Add(new ReviewEntity
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
