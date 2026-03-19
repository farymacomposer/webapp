using System.Diagnostics;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ComposerStreamStore(AppDbContext context)
    {
        public ComposerStreamEntity Create(DateOnly eventDate, ComposerStreamType type, UserEntity createdByUser)
        {
            if (type == ComposerStreamType.Unspecified)
            {
                throw new UnreachableException($"Недопустимый тип стрима '{type}'");
            }

            return context.Add(new ComposerStreamEntity
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public Task<ComposerStreamEntity?> FindById(long id, CancellationToken ct) => context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<ComposerStreamEntity?> FindLive(CancellationToken ct) => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);

        public Task<ComposerStreamEntity?> FindNearest(DateOnly date, CancellationToken ct)
        {
            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= date))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }

        public Task<ComposerStreamEntity?> FindNearest(DateOnly date, ComposerStreamType type, CancellationToken ct)
        {
            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Type == type
                    && x.EventDate >= date
                    && (x.Status == ComposerStreamStatus.Planned || x.Status == ComposerStreamStatus.Live))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }
    }
}