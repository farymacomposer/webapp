using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ComposerStreamRepository(AppDbContext context)
    {
        public ComposerStream Create(DateOnly eventDate, ComposerStreamType type)
        {
            return context.Add(new ComposerStream
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type
            }).Entity;
        }

        public Task<ComposerStream> Get(DateOnly eventDate) => context.ComposerStreams.FirstAsync(x => x.EventDate == eventDate);

        public async Task<ComposerStream> Get(long id) => await context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Стрим не существует", id);

        public Task<ComposerStream?> Find(DateOnly eventDate) => context.ComposerStreams.FirstOrDefaultAsync(x => x.EventDate == eventDate);
        public Task<ComposerStream?> FindLive() => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);

        public Task<ComposerStream[]> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            return context.ComposerStreams
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToArrayAsync();
        }

        public Task<ComposerStream?> FindNearest(DateOnly date)
        {
            return context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= date))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }

        public Task<ComposerStream?> FindNearest(DateOnly date, ComposerStreamType type)
        {
            return context.ComposerStreams
                .Where(x => x.Type == type
                    && x.EventDate >= date
                    && (x.Status == ComposerStreamStatus.Planned || x.Status == ComposerStreamStatus.Live))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }

        public Task<ComposerStream[]> FindLiveAndPlanned()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            return context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .ToArrayAsync();
        }

        public Task<Dictionary<DateOnly, string>> GetLastNicknamesByStreamDate() => context.ComposerStreams
            .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
            .Select(x => new
            {
                x.EventDate,
                x.ProcessedReviewOrders.Where(x => x.Type != ReviewOrderType.OutOfQueue)
                    .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                    .Last().MainNormalizedNickname
            })
            .ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname);
    }
}