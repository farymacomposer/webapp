using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
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

        public async Task<IReadOnlyCollection<ComposerStream>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            return await context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .ToArrayAsync();
        }

        public Task<bool> Exists(DateOnly eventDate) => context.ComposerStreams.AnyAsync(x => x.EventDate == eventDate);
    }
}