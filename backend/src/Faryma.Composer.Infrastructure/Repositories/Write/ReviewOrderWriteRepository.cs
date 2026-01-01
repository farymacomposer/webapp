using Faryma.Composer.Contracts.Exceptions;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
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
                .Include(x => x.Transactions)
                .Include(x => x.Review)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public ReviewOrderEntity Create(
            DateTime createdAt,
            int nominalAmount,
            int payableAmount,
            string? trackUrl,
            string? userComment,
            ReviewOrderType type,
            ComposerStreamEntity stream,
            UserNicknameEntity userNickname)
        {
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
                CreationStream = stream,
                UserNicknames = { userNickname },
            }).Entity;
        }
    }
}