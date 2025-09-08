using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Read
{
    public sealed class ReviewOrder_R_Repository(AppDbContext context)
    {
        public Task<ReviewOrder?> FindInProgress()
        {
            return context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress);
        }

        public async Task<ReviewOrder?> FindLastTaken()
        {
            return await FindInProgress()
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        public async Task<string?> FindLastTakenOutOfQueueNickname()
        {
            return await context.ReviewOrders
                .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.InProgress)
                .Select(x => x.MainNormalizedNickname)
                .FirstOrDefaultAsync()
                ?? await context.ReviewOrders
                    .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .Select(x => x.MainNormalizedNickname)
                    .LastOrDefaultAsync();
        }

        public Task<ReviewOrder[]> GetOrdersToStartStream(long startedStreamId)
        {
            return context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.Payments)
                .Where(x => x.CreationStreamId == startedStreamId
                    && (x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .ToArrayAsync();
        }

        public Task<ReviewOrder[]> GetOrdersToCompleteStream(long completedStreamId)
        {
            return context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Payments)
                .Where(x => (x.CreationStreamId == completedStreamId
                    && (x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                    || (x.ProcessingStreamId == completedStreamId && x.Status == ReviewOrderStatus.Completed))
                .ToArrayAsync();
        }

        public Task<ReviewOrder[]> GetOrdersInQueue()
        {
            return context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Payments)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.ProcessingStream != null
                        && x.ProcessingStream.Status == ComposerStreamStatus.Live
                        && x.Status == ReviewOrderStatus.Completed))
                .ToArrayAsync();
        }
    }
}