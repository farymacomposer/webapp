using System.Diagnostics;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class ReviewOrderStore(AppDbContext context)
    {
        public Task<ReviewOrderEntity?> FindById(long id, CancellationToken ct)
        {
            return context.ReviewOrders
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Transactions)
                .Include(x => x.Review)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public ReviewOrderEntity Create(
            DateTime createdAt,
            int nominalAmount,
            int payableAmount,
            string? trackUrl,
            string? userComment,
            ReviewOrderType type,
            ComposerStreamEntity creationStream,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nominalAmount);

            if (type == ReviewOrderType.Unspecified)
            {
                throw new UnreachableException($"Недопустимый тип заказа разбора трека '{type}'");
            }

            return context.Add(new ReviewOrderEntity
            {
                CreatedAt = createdAt,
                MainNickname = userNickname.Nickname,
                MainNormalizedNickname = userNickname.NormalizedNickname,
                Type = type,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                CategoryType = OrderCategoryType.Unspecified,
                IsFrozen = false,
                TrackUrl = trackUrl,
                NominalAmount = nominalAmount,
                PayableAmount = payableAmount,
                UserComment = userComment,
                CreationStream = creationStream,
                UserNicknames = { userNickname },
                CreatedByUser = createdByUser,
            }).Entity;
        }
    }
}