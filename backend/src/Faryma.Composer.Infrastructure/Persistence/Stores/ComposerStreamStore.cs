using Faryma.Composer.Contracts.Exceptions;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ComposerStreamStore(AppDbContext context)
    {
        public ComposerStreamEntity Create(DateOnly eventDate, ComposerStreamType type)
        {
            if (type == ComposerStreamType.Unspecified)
            {
                throw new InvalidOperationException($"Недопустимый тип стрима '{type}'");
            }

            return context.Add(new ComposerStreamEntity
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type,
            }).Entity;
        }

        public async Task<ComposerStreamEntity> Get(long id) => await context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Стрим не существует", id);

        public Task<ComposerStreamEntity?> FindLive() => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live);

        public Task<ComposerStreamEntity?> FindNearest(DateOnly date)
        {
            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= date))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync();
        }

        public Task<ComposerStreamEntity?> FindNearest(DateOnly date, ComposerStreamType type)
        {
            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Type == type
                    && x.EventDate >= date
                    && (x.Status == ComposerStreamStatus.Planned || x.Status == ComposerStreamStatus.Live))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync();
        }
    }
}