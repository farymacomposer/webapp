using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ComposerStreamStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public ComposerStreamEntity Create(DateOnly eventDate, ComposerStreamType type, UserEntity createdByUser)
        {
            return context.Add(new ComposerStreamEntity
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public Task<ComposerStreamEntity?> FindById(long id, CancellationToken ct = default) => context.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<ComposerStreamEntity?> FindLive(CancellationToken ct = default) => context.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);

        /// <summary>
        /// Возвращает ближайший доступный стрим: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public Task<ComposerStreamEntity?> FindNearest(CancellationToken ct = default)
        {
            DateOnly today = dateTimeService.Today;

            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает ближайший доступный стрим указанного типа: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public Task<ComposerStreamEntity?> FindNearest(ComposerStreamType type, CancellationToken ct = default)
        {
            DateOnly today = dateTimeService.Today;

            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Type == type
                    && (x.Status == ComposerStreamStatus.Live
                        || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today)))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }
    }
}
