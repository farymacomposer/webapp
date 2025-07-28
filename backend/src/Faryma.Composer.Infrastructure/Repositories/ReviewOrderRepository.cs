using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ReviewOrderRepository(AppDbContext context)
    {
        public Task<ReviewOrder?> Find(long id) => context.ReviewOrders.FirstOrDefaultAsync(x => x.Id == id);

        public ReviewOrder CreateOrder(ComposerStream stream, Transaction transaction, ReviewOrderType type, string? trackUrl, string? userComment)
        {
            return context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                NormalizedNickname = transaction.Account.UserNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                ComposerStream = stream,
                UserNicknames = { transaction.Account.UserNickname },
                Payments = { transaction },
            }).Entity;
        }

        public ReviewOrder CreateOrder(ComposerStream stream, UserNickname userNickname, ReviewOrderType type, decimal nominalAmount, string? trackUrl, string? userComment)
        {
            return context.Add(new ReviewOrder
            {
                CreatedAt = DateTime.UtcNow,
                IsFrozen = false,
                Type = type,
                Status = (trackUrl is null) ? ReviewOrderStatus.Preorder : ReviewOrderStatus.Pending,
                NormalizedNickname = userNickname.NormalizedNickname,
                TrackUrl = trackUrl,
                UserComment = userComment,
                ComposerStream = stream,
                UserNicknames = { userNickname },
                NominalAmount = nominalAmount,
            }).Entity;
        }
    }
}