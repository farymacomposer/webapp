using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewRepository(AppDbContext context)
    {
        public Review Add(Review review)
        {
            return context.Reviews.Add(review).Entity;
        }
    }
}