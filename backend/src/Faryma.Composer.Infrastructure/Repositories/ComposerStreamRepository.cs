using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ComposerStreamRepository(AppDbContext context)
    {
        public Task<ComposerStream> Get(DateOnly eventDate) => context.ComposerStreams.FirstAsync(x => x.EventDate == eventDate);

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

        public Task<ComposerStream?> FindNearest(DateOnly dateFrom)
        {
            DateOnly dateTo = dateFrom.AddDays(6);

            return context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.EventDate >= dateFrom && x.EventDate <= dateTo))
                .OrderBy(x => x.EventDate)
                .FirstOrDefaultAsync();
        }
    }
}