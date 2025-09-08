using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Read
{
    public sealed class ComposerStream_R_Repository(AppDbContext context)
    {
        public Task<ComposerStream[]> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToArrayAsync();
        }

        public Task<ComposerStream?> FindLive()
        {
            return context.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);
        }

        public Task<ComposerStream?> FindNearest(DateOnly date)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= date))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }

        public Task<ComposerStream[]> FindLiveAndPlanned()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .ToArrayAsync();
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