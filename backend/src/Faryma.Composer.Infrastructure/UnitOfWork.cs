using Faryma.Composer.Infrastructure.Repositories.Read;
using Faryma.Composer.Infrastructure.Repositories.ReadWrite;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,

        ComposerStream_R_Repository composerStream_R,
        ReviewOrder_R_Repository reviewOrder_R,
        UserNickname_R_Repository userNickname_R,

        ComposerStream_RW_Repository composerStream_RW,
        Review_RW_Repository review_RW,
        ReviewOrder_RW_Repository reviewOrder_RW,
        Transaction_RW_Repository transaction_RW,
        UserAccount_RW_Repository userAccount_RW,
        UserNickname_RW_Repository userNickname_RW)
    {
        public ComposerStream_R_Repository ComposerStream_R { get; } = composerStream_R;
        public ReviewOrder_R_Repository ReviewOrder_R { get; } = reviewOrder_R;
        public UserNickname_R_Repository UserNickname_R { get; } = userNickname_R;

        public ComposerStream_RW_Repository ComposerStream_RW { get; } = composerStream_RW;
        public Review_RW_Repository Review_RW { get; } = review_RW;
        public ReviewOrder_RW_Repository ReviewOrder_RW { get; } = reviewOrder_RW;
        public Transaction_RW_Repository Transaction_RW { get; } = transaction_RW;
        public UserAccount_RW_Repository UserAccount_RW { get; } = userAccount_RW;
        public UserNickname_RW_Repository UserNickname_RW { get; } = userNickname_RW;

        public Task<IDbContextTransaction> BeginTransaction() => context.Database.BeginTransactionAsync();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Remove(entity);
    }
}