using Faryma.Composer.Infrastructure.Persistence.Queries;
using Faryma.Composer.Infrastructure.Persistence.Stores;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,

        ComposerStreamQueries composerStreamQueries,
        ReviewOrderQueries reviewOrderQueries,
        UserNicknameQueries userNicknameQueries,

        ComposerStreamStore composerStreamStore,
        ReviewStore reviewStore,
        ReviewOrderStore reviewOrderStore,
        TransactionStore transactionStore,
        UserNicknameStore userNicknameStore
        )
    {
        public ComposerStreamQueries ComposerStreamQueries { get; } = composerStreamQueries;
        public ReviewOrderQueries ReviewOrderQueries { get; } = reviewOrderQueries;
        public UserNicknameQueries UserNicknameQueries { get; } = userNicknameQueries;

        public ComposerStreamStore ComposerStreamStore { get; } = composerStreamStore;
        public ReviewStore ReviewStore { get; } = reviewStore;
        public ReviewOrderStore ReviewOrderStore { get; } = reviewOrderStore;
        public TransactionStore TransactionStore { get; } = transactionStore;
        public UserNicknameStore UserNicknameStore { get; } = userNicknameStore;

        public Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default) => context.Database.BeginTransactionAsync(cancellationToken);
        public Task<int> SaveChanges(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Remove(entity);
    }
}