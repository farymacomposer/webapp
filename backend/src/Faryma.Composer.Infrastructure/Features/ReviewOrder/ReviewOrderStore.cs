using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.ReviewOrder
{
    public sealed class ReviewOrderStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public ReviewOrderEntity CreateOrder(
            ReviewOrderType type,
            ReviewOrderStatus status,
            string? trackUrl,
            int? trackDurationSeconds,
            long nominalPrice,
            long payableAmount,
            string? userComment,
            ComposerStreamEntity creationStream,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nominalPrice);
            ArgumentOutOfRangeException.ThrowIfNegative(payableAmount);

            if (!Enum.IsDefined(type) || type == ReviewOrderType.Unspecified)
            {
                throw new ArgumentException("Тип заказа должен быть указан", nameof(type));
            }

            if (!Enum.IsDefined(status) || status == ReviewOrderStatus.Unspecified)
            {
                throw new ArgumentException("Статус заказа должен быть указан", nameof(type));
            }

            return context.Add(new ReviewOrderEntity
            {
                CreatedAt = dateTimeService.Now,
                MainNickname = userNickname.Nickname,
                MainNormalizedNickname = userNickname.NormalizedNickname,
                Type = type,
                Status = status,
                QueueCategory = QueueCategory.Unspecified,
                IsFrozen = false,
                TrackUrl = trackUrl,
                TrackDurationSeconds = trackDurationSeconds,
                Price = nominalPrice,
                PayableAmount = payableAmount,
                UserComment = userComment,
                CreationStream = creationStream,
                UserNicknames = { userNickname },
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public async Task<ReviewOrderEntity> GetOrder(long id, CancellationToken ct)
        {
            return await FindOrderById(id, ct)
                ?? throw new NotFoundException($"Заказ id: {id} не найден");
        }

        public Task<ReviewOrderEntity?> FindOrderById(long id, CancellationToken ct)
        {
            IQueryable<ReviewOrderEntity> query = context.ReviewOrders
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Transactions)
                .Include(x => x.DetailedReviewPayment)
                .ThenInclude(x => x!.Transactions)
                .Include(x => x.CoverageRedemption)
                .Include(x => x.Review)
                .Where(x => x.Id == id);

            return query.FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Возвращает заказ в статусе InProgress, если он существует
        /// </summary>
        public Task<ReviewOrderEntity?> FindOrderInProgress(CancellationToken ct)
        {
            return context.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress, ct);
        }

        /// <summary>
        /// Возвращает запущенный благотворительный стрим
        /// </summary>
        public async Task<ComposerStreamEntity> GetLiveCharityStream(CancellationToken ct)
        {
            return await context.ComposerStreams
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live && x.Type == ComposerStreamType.Charity, ct)
                ?? throw new NotFoundException("Нет запущенного благотворительного стрима");
        }

        /// <summary>
        /// Возвращает ближайший доступный стрим: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public async Task<ComposerStreamEntity> GetNearestStream(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IOrderedQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return await query.FirstOrDefaultAsync(ct)
                ?? throw new NotFoundException("Нет доступного ближайшего стрима");
        }

        /// <summary>
        /// Возвращает ближайший доступный стрим: Live или ближайший Planned на сегодня/будущее, с учетом заказов пользователя
        /// </summary>
        public async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname, CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IQueryable<ComposerStreamEntity> query = context.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today));

            bool userNicknameHasOrders = await context.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0, ct);
            if (userNicknameHasOrders)
            {
                query = query.Where(x => x.Type == ComposerStreamType.Donation);
            }

            query = query.OrderBy(x => x.EventDate);

            return await query.FirstOrDefaultAsync(ct);
        }

        public ReviewOrderDetailedReviewPaymentEntity CreateDetailedReviewPayment(
            ReviewOrderEntity order,
            long price,
            UserEntity createdByUser)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

            return context.Add(new ReviewOrderDetailedReviewPaymentEntity
            {
                CreatedAt = dateTimeService.Now,
                ReviewOrder = order,
                Price = price,
                CreatedByUser = createdByUser,
            }).Entity;
        }
    }
}
