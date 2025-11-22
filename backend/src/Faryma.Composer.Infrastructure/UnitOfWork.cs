using Faryma.Composer.Infrastructure.Repositories.Read;
using Faryma.Composer.Infrastructure.Repositories.Write;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,

        ComposerStreamReadRepository composerStreamRead,
        ReviewOrderReadRepository reviewOrderRead,
        UserNicknameReadRepository userNicknameRead,

        ComposerStreamWriteRepository composerStreamWrite,
        ReviewWriteRepository reviewWrite,
        ReviewOrderWriteRepository reviewOrderWrite,
        TransactionWriteRepository transactionWrite,
        UserAccountWriteRepository userAccountWrite,
        UserNicknameWriteRepository userNicknameWrite
        )
    {
        public ComposerStreamReadRepository ComposerStreamRead { get; } = composerStreamRead;
        public ReviewOrderReadRepository ReviewOrderRead { get; } = reviewOrderRead;
        public UserNicknameReadRepository UserNicknameRead { get; } = userNicknameRead;

        public ComposerStreamWriteRepository ComposerStreamWrite { get; } = composerStreamWrite;
        public ReviewWriteRepository ReviewWrite { get; } = reviewWrite;
        public ReviewOrderWriteRepository ReviewOrderWrite { get; } = reviewOrderWrite;
        public TransactionWriteRepository TransactionWrite { get; } = transactionWrite;
        public UserAccountWriteRepository UserAccountWrite { get; } = userAccountWrite;
        public UserNicknameWriteRepository UserNicknameWrite { get; } = userNicknameWrite;

        public Task<IDbContextTransaction> BeginTransaction() => context.Database.BeginTransactionAsync();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Remove(entity);
    }
}