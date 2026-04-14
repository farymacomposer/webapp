using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Queries
{
    public sealed class ComposerStreamQueries(AppDbContext context, DateTimeService dateTimeService)
    {
        /// <summary>
        /// Возвращает стримы в указанном диапазоне дат
        /// </summary>
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct)
        {
            return context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .OrderBy(x => x.EventDate)
                .ToListAsync(ct);
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

        /// <summary>
        /// Возвращает ближайший доступный стрим: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public Task<ComposerStreamEntity?> FindNearest(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает дату ближайшего доступного стрима или DateOnly.MinValue, если стримов нет
        /// </summary>
        public async Task<DateOnly> GetNearestStreamDate(CancellationToken ct = default)
        {
            ComposerStreamEntity? nearestStream = await FindNearest(ct);

            return nearestStream?.EventDate ?? DateOnly.MinValue;
        }

        /// <summary>
        /// Возвращает список актуальных стримов: Live и Planned на сегодня/будущее
        /// </summary>
        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return query.ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает последний выданный никнейм по каждой дате стрима для приоритетного алгоритма очереди
        /// </summary>
        public Task<Dictionary<DateOnly, string>> GetLastNicknamesByStreamDate(CancellationToken ct = default)
        {
            var query = context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                    && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder || x.Status == ReviewOrderStatus.Pending))
                .Select(x => new
                {
                    x.EventDate,
                    x.ProcessedReviewOrders
                        .Where(x => x.Type != ReviewOrderType.OutOfQueue)
                        .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                        .Last().MainNormalizedNickname
                });

            return query.ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname, ct);
        }
    }
}