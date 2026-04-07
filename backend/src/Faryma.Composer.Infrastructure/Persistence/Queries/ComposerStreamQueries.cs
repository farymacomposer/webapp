using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Queries
{
    public sealed class ComposerStreamQueries(AppDbContext context)
    {
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToListAsync(ct);
        }

        public Task<ComposerStreamEntity?> FindLive(CancellationToken ct)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);
        }

        public Task<ComposerStreamEntity?> FindNearest(CancellationToken ct)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }

        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned(CancellationToken ct)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            IQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today));

            return query.ToListAsync(ct);
        }

        public Task<Dictionary<DateOnly, string>> GetLastNicknamesByStreamDate(CancellationToken ct)
        {
            var query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                    && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.ProcessedReviewOrders
                        .Where(x => x.Type != ReviewOrderType.OutOfQueue)
                        .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                        .Last().MainNormalizedNickname
                });

            return query.ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname, ct);
        }
    }
}