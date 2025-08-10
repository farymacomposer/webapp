using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ComposerStreamRepository(AppDbContext context)
    {
        public Task<ComposerStream> Get(DateOnly eventDate) => context.ComposerStreams.FirstAsync(x => x.EventDate == eventDate);
        public Task<ComposerStream?> Find(DateOnly eventDate) => context.ComposerStreams.FirstOrDefaultAsync(x => x.EventDate == eventDate);
        public Task<ComposerStream?> FindLive() => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);

        public async Task<ComposerStream> Get(long composerStreamId) => await context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == composerStreamId)
            ?? throw new NotFoundException($"Стрим Id: {composerStreamId}, не существует");

        public ComposerStream Create(DateOnly eventDate, ComposerStreamType type)
        {
            return context.Add(new ComposerStream
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type
            }).Entity;
        }

        public async Task<IReadOnlyCollection<ComposerStream>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            return await context.ComposerStreams
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToArrayAsync();
        }

        public Task<ComposerStream?> FindNearestInWeekRange(DateOnly dateFrom)
        {
            return context.ComposerStreams
                .Where(x => x.Status != ComposerStreamStatus.Canceled
                    && (x.Status == ComposerStreamStatus.Live || x.EventDate >= dateFrom))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ComposerStream>> FindCurrentAndScheduled()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            return await context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .ToArrayAsync();
        }
    }
}