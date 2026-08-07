using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed partial class OrderQueueService
    {
        /// <summary>
        /// Возвращает текущий заказ в работе, либо последний завершенный заказ
        /// </summary>
        private static async Task<ReviewOrderEntity?> FindLastTaken(UnitOfWork uow)
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
        private static async Task<ReviewOrderEntity?> FindLastTakenDebt(UnitOfWork uow)
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
        private static async Task<ReviewOrderEntity?> FindLastTakenOutOfQueue(UnitOfWork uow)
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
        private static Task<List<ReviewOrderEntity>> GetOrdersInQueue(UnitOfWork uow)
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
        private static Task<List<ReviewOrderEntity>> GetOrdersToStartStream(UnitOfWork uow, long streamId)
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
        private static Task<List<ReviewOrderEntity>> GetOrdersToCompleteStream(UnitOfWork uow, long streamId)
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
