using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Queries
{
    public sealed class ReviewOrderQueries(AppDbContext context)
    {
        /// <summary>
        /// Проверяет, есть ли у стрима активные созданные заказы
        /// </summary>
        public Task<bool> ExistsActiveCreatedOrdersForStream(long streamId, CancellationToken ct = default)
        {
            return context.ReviewOrders
                .AnyAsync(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment), ct);
        }

        /// <summary>
        /// Возвращает заказ в статусе InProgress, если он существует
        /// </summary>
        public Task<ReviewOrderEntity?> FindInProgress(CancellationToken ct)
        {
            return context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress, ct);
        }

        /// <summary>
        /// Возвращает текущий заказ в работе, либо последний завершенный заказ
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTaken(CancellationToken ct = default)
        {
            return await FindInProgress(ct)
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает текущий/последний взятый заказ категории Debt
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTakenDebt(CancellationToken ct = default)
        {
            return await context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.QueueCategory == QueueCategory.Debt && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync(ct)
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Include(x => x.CreationStream)
                    .Where(x => x.QueueCategory == QueueCategory.Debt && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает текущий/последний взятый заказ типа OutOfQueue
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTakenOutOfQueue(CancellationToken ct = default)
        {
            return await context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync(ct)
                ?? await context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает заказы, которые нужно обновить при старте стрима
        /// </summary>
        public Task<List<ReviewOrderEntity>> GetOrdersToStartStream(long streamId, CancellationToken ct = default)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment));

            return IncludePricingSources(query).ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает заказы, которые нужно обновить при завершении стрима
        /// </summary>
        public Task<List<ReviewOrderEntity>> GetOrdersToCompleteStream(long streamId, CancellationToken ct = default)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Where(x => (x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment))
                    || (x.ProcessingStreamId == streamId && x.Status == ReviewOrderStatus.Completed));

            return IncludePricingSources(query).ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает заказы, которые участвуют в расчете текущей очереди
        /// </summary>
        public Task<List<ReviewOrderEntity>> GetOrdersInQueue(CancellationToken ct = default)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
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

            return IncludePricingSources(query).ToListAsync(ct);
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
