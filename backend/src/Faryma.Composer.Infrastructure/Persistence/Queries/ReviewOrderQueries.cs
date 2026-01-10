using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Queries
{
    public sealed class ReviewOrderQueries(AppDbContext context)
    {
        public Task<ReviewOrderEntity?> FindInProgress()
        {
            return context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress);
        }

        public async Task<ReviewOrderEntity?> FindLastTaken()
        {
            return await FindInProgress()
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        public async Task<ReviewOrderEntity?> FindLastTakenDebt()
        {
            return await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.CategoryType == OrderCategoryType.Debt && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Include(x => x.CreationStream)
                    .Where(x => x.CategoryType == OrderCategoryType.Debt && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        public async Task<ReviewOrderEntity?> FindLastTakenOutOfQueue()
        {
            return await context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        public Task<List<ReviewOrderEntity>> GetOrdersToStartStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.Transactions)
                .Where(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending));

            return query.ToListAsync();
        }

        public Task<List<ReviewOrderEntity>> GetOrdersToCompleteStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Transactions)
                .Where(x => (x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                    || (x.ProcessingStreamId == streamId && x.Status == ReviewOrderStatus.Completed));

            return query.ToListAsync();
        }

        public Task<List<ReviewOrderEntity>> GetOrdersInQueue()
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Transactions)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.ProcessingStream != null
                        && x.ProcessingStream.Status == ComposerStreamStatus.Live
                        && x.Status == ReviewOrderStatus.Completed));

            return query.ToListAsync();
        }
    }
}