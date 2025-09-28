using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class ComposerStream_RW_Repository(AppDbContext context)
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

        public async Task<ComposerStream> Get(long id) => await context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Стрим не существует", id);

        public Task<ComposerStream?> FindLive() => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);

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
    }
}