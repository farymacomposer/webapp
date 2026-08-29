using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.OrderQueue
{
    public sealed class OrderQueueStore(AppDbContext appDbContext, DateTimeContext dateTimeContext)
    {
        /// <summary>
        /// Возвращает дату ближайшего доступного стрима или DateOnly.MinValue, если стримов нет
        /// </summary>
        public async Task<DateOnly> FindNearestStreamDate()
        {
            DateOnly today = dateTimeContext.Today;

            IOrderedQueryable<ComposerStreamEntity> query = appDbContext.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            ComposerStreamEntity? nearestStream = await query.FirstOrDefaultAsync();

            return nearestStream?.EventDate ?? DateOnly.MinValue;
        }

        /// <summary>
        /// Возвращает последний выданный никнейм по каждой дате стрима для приоритетного алгоритма очереди
        /// </summary>
        public Task<Dictionary<DateOnly, string>> FindLastNicknamesByStreamDate()
        {
            var query = appDbContext.ComposerStreams
                .AsNoTracking()
                .Where(x => x.ProcessedReviewOrders.Any(x => x.Type != ReviewOrderType.OutOfQueue)
                    && x.CreatedReviewOrders.Any(x => x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment))
                .Select(x => new
                {
                    x.EventDate,
                    x.ProcessedReviewOrders
                        .Where(x => x.Type != ReviewOrderType.OutOfQueue)
                        .OrderBy(x => (x.Status == ReviewOrderStatus.Completed) ? x.CompletedAt : DateTime.MaxValue)
                        .Last().MainNormalizedNickname
                });

            return query.ToDictionaryAsync(k => k.EventDate, v => v.MainNormalizedNickname);
        }

        /// <summary>
        /// Возвращает текущий заказ в работе, либо последний завершенный заказ
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTaken()
        {
            return await appDbContext.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress)
                ?? await appDbContext.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        /// <summary>
        /// Возвращает текущий/последний взятый заказ категории Debt
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTakenDebt()
        {
            return await appDbContext.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.QueueCategory == QueueCategory.Debt && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await appDbContext.ReviewOrders
                    .AsNoTracking()
                    .Include(x => x.CreationStream)
                    .Where(x => x.QueueCategory == QueueCategory.Debt && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        /// <summary>
        /// Возвращает текущий/последний взятый заказ типа OutOfQueue
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTakenOutOfQueue()
        {
            return await appDbContext.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await appDbContext.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        /// <summary>
        /// Возвращает заказы, которые участвуют в расчете текущей очереди
        /// </summary>
        public Task<List<ReviewOrderEntity>> FindOrdersInQueue()
        {
            IQueryable<ReviewOrderEntity> query = appDbContext.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Where(x => x.Status == ReviewOrderStatus.Preorder
                    || x.Status == ReviewOrderStatus.Pending
                    || x.Status == ReviewOrderStatus.AwaitingPayment
                    || x.Status == ReviewOrderStatus.InProgress
                    || (x.ProcessingStream != null
                        && x.ProcessingStream.Status == ComposerStreamStatus.Live
                        && x.Status == ReviewOrderStatus.Completed));

            return IncludePricingSources(query).ToListAsync();
        }

        /// <summary>
        /// Возвращает заказы, которые нужно обновить при старте стрима
        /// </summary>
        public Task<List<ReviewOrderEntity>> FindOrdersToStartStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = appDbContext.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment));

            return IncludePricingSources(query).ToListAsync();
        }

        /// <summary>
        /// Возвращает заказы, которые нужно обновить при завершении стрима
        /// </summary>
        public Task<List<ReviewOrderEntity>> FindOrdersToCompleteStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = appDbContext.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Where(x => (x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment))
                    || (x.ProcessingStreamId == streamId && x.Status == ReviewOrderStatus.Completed));

            return IncludePricingSources(query).ToListAsync();
        }

        private static IQueryable<ReviewOrderEntity> IncludePricingSources(IQueryable<ReviewOrderEntity> query)
        {
            return query
                .Include(x => x.Transactions)
                .Include(x => x.DetailedReviewPayment)
                .ThenInclude(x => x!.Transactions)
                .Include(x => x.CoverageRedemption);
        }
    }
}
