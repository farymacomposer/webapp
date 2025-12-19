using Faryma.Composer.Contracts.Exceptions;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class ReviewOrderWriteRepository(AppDbContext context)
    {
        public async Task<ReviewOrderEntity> Get(long id) => await Find(id)
            ?? throw new NotFoundException("Заказ разбора трека не существует", id);

        public Task<ReviewOrderEntity?> Find(long id)
        {
            return context.ReviewOrders
                .Include(x => x.CreationStream)
                .Include(x => x.ProcessingStream)
                .Include(x => x.Payments)
                .Include(x => x.Review)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public ReviewOrderEntity CreateDonation(
            ComposerStreamEntity stream,
            TransactionEntity transaction,
            int nominalAmount,
            string? trackUrl,
            string? userComment)
        {
            return context.Add(new ReviewOrderEntity
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = ReviewOrderType.Donation,
                CategoryType = OrderCategoryType.Unspecified,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = transaction.Account.UserNickname.Nickname,
                MainNormalizedNickname = transaction.Account.UserNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                CreationStream = stream,
                UserNicknames = { transaction.Account.UserNickname },
                NominalAmount = nominalAmount,
                Payments = { transaction },
            }).Entity;
        }

        public ReviewOrderEntity CreateFree(
            ComposerStreamEntity stream,
            UserNicknameEntity userNickname,
            int nominalAmount,
            string? trackUrl,
            string? userComment,
            ReviewOrderType type)
        {
            return context.Add(new ReviewOrderEntity
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                CategoryType = OrderCategoryType.Unspecified,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = userNickname.Nickname,
                MainNormalizedNickname = userNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                CreationStream = stream,
                UserNicknames = { userNickname },
                NominalAmount = nominalAmount,
            }).Entity;
        }
    }
}