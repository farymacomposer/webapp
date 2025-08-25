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

        public Task<ComposerStream?> FindNearest(DateOnly today)
        {
            return context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
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
    }
}