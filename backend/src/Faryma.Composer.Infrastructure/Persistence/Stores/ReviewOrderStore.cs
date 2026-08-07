using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ReviewOrderStore(
        AppDbContext context,
        DateTimeService dateTimeService)
    {
        public Task<ReviewOrderEntity?> FindById(long id, CancellationToken ct = default)
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

        public async Task<ReviewOrderEntity> Get(long id, CancellationToken ct)
        {
            return await FindById(id, ct)
                ?? throw new NotFoundException($"Заказ id: {id} не найден");
        }

        public ReviewOrderDetailedReviewPaymentEntity CreateDetailedReviewPayment(
            ReviewOrderEntity order,
            long amount,
            UserEntity createdByUser)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            return context.Add(new ReviewOrderDetailedReviewPaymentEntity
            {
                CreatedAt = dateTimeService.Now,
                ReviewOrder = order,
                Price = amount,
                CreatedByUser = createdByUser,
            }).Entity;
        }

        public ReviewOrderEntity Create(
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
    }
}
