using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.ComposerStream
{
    public sealed class ComposerStreamStore(
        AppDbContext appDbContext,
        DateTimeService dateTimeService)
    {
        public ComposerStreamEntity CreateStream(DateOnly eventDate, ComposerStreamType type, UserEntity createdByUser)
        {
            if (!Enum.IsDefined(type) || type == ComposerStreamType.Unspecified)
            {
                throw new ArgumentException("Тип стрима должен быть указан", nameof(type));
            }

            return appDbContext.Add(new ComposerStreamEntity
            {
                EventDate = eventDate,
                Status = ComposerStreamStatus.Planned,
                Type = type,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public async Task<ComposerStreamEntity> GetStream(long id, CancellationToken ct)
        {
            return await appDbContext.ComposerStreams.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new NotFoundException($"Стрим id: {id} не найден");
        }

        /// <summary>
        /// Возвращает текущий стрим в статусе Live, если он существует
        /// </summary>
        public Task<ComposerStreamEntity?> FindLiveStream(CancellationToken ct)
        {
            return appDbContext.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);
        }

        /// <summary>
        /// Возвращает стримы в указанном диапазоне дат
        /// </summary>
        public async Task<IReadOnlyCollection<ComposerStreamEntity>> FindStreams(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct)
        {
            return await appDbContext.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .OrderBy(x => x.EventDate)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает список актуальных стримов: Live и Planned на сегодня/будущее
        /// </summary>
        public async Task<IReadOnlyCollection<ComposerStreamEntity>> FindLiveAndPlannedStreams(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IQueryable<ComposerStreamEntity> query = appDbContext.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return await query.ToListAsync(ct);
        }

        /// <summary>
        /// Проверяет, есть-ли у стрима активные созданные заказы
        /// </summary>
        public Task<bool> ExistsActiveCreatedOrdersForStream(long streamId, CancellationToken ct)
        {
            return appDbContext.ReviewOrders
                .AsNoTracking()
                .AnyAsync(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment), ct);
        }
    }
}
