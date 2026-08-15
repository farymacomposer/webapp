using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.OrderQueue
{
    public sealed class ReviewOrderQueries(UnitOfWork uow)
    {
        /// <summary>
        /// Возвращает текущий заказ в работе, либо последний завершенный заказ
        /// </summary>
        public async Task<ReviewOrderEntity?> FindLastTaken()
        {
            return await uow.Context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress)
                ?? await uow.Context.ReviewOrders
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
            return await uow.Context.ReviewOrders
                .AsNoTracking()
                .Include(x => x.CreationStream)
                .Where(x => x.QueueCategory == QueueCategory.Debt && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await uow.Context.ReviewOrders
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
            return await uow.Context.ReviewOrders
                .AsNoTracking()
                .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.InProgress)
                .FirstOrDefaultAsync()
                ?? await uow.Context.ReviewOrders
                    .AsNoTracking()
                    .Where(x => x.Type == ReviewOrderType.OutOfQueue && x.Status == ReviewOrderStatus.Completed)
                    .OrderBy(x => x.CompletedAt)
                    .LastOrDefaultAsync();
        }

        /// <summary>
        /// Возвращает заказы, которые участвуют в расчете текущей очереди
        /// </summary>
        public Task<List<ReviewOrderEntity>> GetOrdersInQueue()
        {
            IQueryable<ReviewOrderEntity> query = uow.Context.ReviewOrders
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
        public Task<List<ReviewOrderEntity>> GetOrdersToStartStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = uow.Context.ReviewOrders
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
        public Task<List<ReviewOrderEntity>> GetOrdersToCompleteStream(long streamId)
        {
            IQueryable<ReviewOrderEntity> query = uow.Context.ReviewOrders
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
