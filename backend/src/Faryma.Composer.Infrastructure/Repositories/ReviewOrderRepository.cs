using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewOrderRepository(AppDbContext context)
    {
        public async Task<ReviewOrder> Get(long reviewOrderId) => await Find(reviewOrderId)
            ?? throw new NotFoundException($"Заказ разбора трека Id: {reviewOrderId}, не существует");

        public Task<ReviewOrder?> Find(long id) => context.ReviewOrders
            .Include(x => x.ComposerStream)
            .Include(x => x.UserNicknames)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);

        public Task<ReviewOrder?> FindAnotherOrderInProgress(long reviewOrderId) =>
            context.ReviewOrders.FirstOrDefaultAsync(x => x.Id != reviewOrderId && x.Status == ReviewOrderStatus.InProgress);

        public ReviewOrder CreateDonation(ComposerStream stream, Transaction transaction, ReviewOrderType type, string? trackUrl, string? userComment)
        {
            return context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = transaction.Account.UserNickname.Nickname,
                MainNormalizedNickname = transaction.Account.UserNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                ComposerStream = stream,
                UserNicknames = { transaction.Account.UserNickname },
                Payments = { transaction },
            }).Entity;
        }

        public ReviewOrder CreateFree(ComposerStream stream, UserNickname userNickname, ReviewOrderType type, decimal nominalAmount, string? trackUrl, string? userComment)
        {
            return context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                MainNickname = userNickname.Nickname,
                MainNormalizedNickname = userNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                ComposerStream = stream,
                UserNicknames = { userNickname },
                NominalAmount = nominalAmount,
            }).Entity;
        }
    }
}