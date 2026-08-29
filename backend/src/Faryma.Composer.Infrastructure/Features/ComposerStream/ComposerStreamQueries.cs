using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.ComposerStream
{
    public sealed class ComposerStreamQueries(
        AppDbContext context,
        DateTimeService dateTimeService)
    {
        /// <summary>
        /// Возвращает стримы в указанном диапазоне дат
        /// </summary>
        public async Task<IReadOnlyCollection<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct)
        {
            return await context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .OrderBy(x => x.EventDate)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает список актуальных стримов: Live и Planned на сегодня/будущее
        /// </summary>
        public async Task<IReadOnlyCollection<ComposerStreamEntity>> FindLiveAndPlanned(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return await query.ToListAsync(ct);
        }

        /// <summary>
        /// Проверяет, есть ли у стрима активные созданные заказы
        /// </summary>
        public Task<bool> ExistsActiveCreatedOrdersForStream(long streamId, CancellationToken ct)
        {
            return context.ReviewOrders
                .AnyAsync(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment), ct);
        }

        /// <summary>
        /// Возвращает текущий стрим в статусе Live, если он существует
        /// </summary>
        public Task<ComposerStreamEntity?> FindLive(CancellationToken ct)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);
        }
    }
}
