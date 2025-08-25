using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewOrderRepository(AppDbContext context)
    {
        public async Task<ReviewOrder> Get(long id) => await Find(id)
            ?? throw new NotFoundException("Заказ разбора трека не существует", id);

        public Task<ReviewOrder?> Find(long id) => context.ReviewOrders
            .Include(x => x.CreationStream)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);

        public Task<ReviewOrder[]> GetOrdersForStream(long creationStreamId) => context.ReviewOrders
            .AsNoTracking()
            .Include(x => x.CreationStream)
            .Include(x => x.Payments)
            .Where(x => x.CreationStreamId == creationStreamId)
            .ToArrayAsync();

        public Task<ReviewOrder?> FindAnotherOrderInProgress(long id) => context.ReviewOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id != id && x.Status == ReviewOrderStatus.InProgress);

        public ReviewOrder CreateDonation(
            ComposerStream stream,
            Transaction transaction,
            int nominalAmount,
            string? trackUrl,
            string? userComment)
        {
            return context.Add(new ReviewOrder
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

        public ReviewOrder CreateFree(
            ComposerStream stream,
            UserNickname userNickname,
            int nominalAmount,
            string? trackUrl,
            string? userComment,
            ReviewOrderType type)
        {
            return context.Add(new ReviewOrder
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