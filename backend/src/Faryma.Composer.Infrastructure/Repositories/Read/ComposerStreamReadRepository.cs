using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Read
{
    public sealed class ComposerStreamReadRepository(AppDbContext context)
    {
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToListAsync();
        }

        public Task<ComposerStreamEntity?> FindLive()
        {
            return context.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);
        }

        public Task<ComposerStreamEntity?> FindNearest()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }

        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .ToListAsync();
        }

        public Task<Dictionary<DateOnly, string>> GetLastNicknamesByStreamDate()
        {
            return context.ComposerStreams
                .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                    && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.ProcessedReviewOrders
                        .Where(x => x.Type != ReviewOrderType.OutOfQueue)
                        .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                        .Last().MainNormalizedNickname
                })
                .ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname);
        }
    }
}