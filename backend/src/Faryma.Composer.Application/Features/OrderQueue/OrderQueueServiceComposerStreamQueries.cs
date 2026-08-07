using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed partial class OrderQueueService
    {
        /// <summary>
        /// Возвращает дату ближайшего доступного стрима или DateOnly.MinValue, если стримов нет
        /// </summary>
        private static async Task<DateOnly> GetNearestStreamDate(UnitOfWork uow)
        {
            DateOnly today = uow.DateTimeService.Today;

            IOrderedQueryable<ComposerStreamEntity> query = uow.Context.ComposerStreams
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
        private static Task<Dictionary<DateOnly, string>> GetLastNicknamesByStreamDate(UnitOfWork uow)
        {
            var query = uow.Context.ComposerStreams
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
    }
}
