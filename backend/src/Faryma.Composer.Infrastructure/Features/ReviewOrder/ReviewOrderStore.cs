using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.ReviewOrder
{
    public sealed class ReviewOrderStore(AppDbContext appDbContext, DateTimeContext dateTimeContext)
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
                throw new ArgumentException("Статус заказа должен быть указан", nameof(status));
            }

            return appDbContext.Add(new ReviewOrderEntity
            {
                CreatedAt = dateTimeContext.Now,
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
            IQueryable<ReviewOrderEntity> query = appDbContext.ReviewOrders
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
            return appDbContext.ReviewOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ReviewOrderStatus.InProgress, ct);
        }

        public ReviewOrderDetailedReviewPaymentEntity CreateDetailedReviewPayment(
            ReviewOrderEntity order,
            long price,
            UserEntity createdByUser)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

            return appDbContext.Add(new ReviewOrderDetailedReviewPaymentEntity
            {
                CreatedAt = dateTimeContext.Now,
                ReviewOrder = order,
                Price = price,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        /// <summary>
        /// Возвращает запущенный стрим
        /// </summary>
        public async Task<ComposerStreamEntity> GetLiveStream(CancellationToken ct)
        {
            return await appDbContext.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct)
                ?? throw new NotFoundException("Нет запущенного стрима");
        }

        /// <summary>
        /// Возвращает запущенный благотворительный стрим
        /// </summary>
        public async Task<ComposerStreamEntity> GetLiveCharityStream(CancellationToken ct)
        {
            return await appDbContext.ComposerStreams.FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live && x.Type == ComposerStreamType.Charity, ct)
                ?? throw new NotFoundException("Нет запущенного благотворительного стрима");
        }

        public Task<bool> HasOrders(UserNicknameEntity userNickname, CancellationToken ct) =>
            appDbContext.UserNicknames.AnyAsync(x => x.Id == userNickname.Id && x.ReviewOrders.Count > 0, ct);

        /// <summary>
        /// Возвращает ближайший доступный стрим: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public Task<ComposerStreamEntity> GetNearestStream(CancellationToken ct) => GetNearestStreamInternal(type: null, ct);

        /// <summary>
        /// Возвращает ближайший доступный стрим указанного типа: Live или ближайший Planned на сегодня/будущее
        /// </summary>
        public Task<ComposerStreamEntity> GetNearestStream(ComposerStreamType type, CancellationToken ct)
        {
            if (!Enum.IsDefined(type) || type == ComposerStreamType.Unspecified)
            {
                throw new ArgumentException("Тип стрима должен быть указан", nameof(type));
            }

            return GetNearestStreamInternal(type, ct);
        }

        private async Task<ComposerStreamEntity> GetNearestStreamInternal(ComposerStreamType? type, CancellationToken ct)
        {
            DateOnly today = dateTimeContext.Today;

            IQueryable<ComposerStreamEntity> query = appDbContext.ComposerStreams
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today));

            if (type is { } streamType)
            {
                query = query.Where(x => x.Type == streamType);
            }

            return await query.OrderBy(x => x.EventDate).FirstOrDefaultAsync(ct)
                ?? throw new NotFoundException("Нет доступного ближайшего стрима");
        }
    }
}
