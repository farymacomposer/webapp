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
        RefreshTokenStore refreshTokenStore,
        ReviewStore reviewStore,
        ReviewOrderStore reviewOrderStore,
        TransactionStore transactionStore,
        UserEntitlementStore userEntitlementStore,
        UserNicknameStore userNicknameStore
        )
    {
        public ComposerStreamQueries ComposerStreamQueries { get; } = composerStreamQueries;
        public ReviewOrderQueries ReviewOrderQueries { get; } = reviewOrderQueries;
        public UserNicknameQueries UserNicknameQueries { get; } = userNicknameQueries;

        public ComposerStreamStore ComposerStreamStore { get; } = composerStreamStore;
        public RefreshTokenStore RefreshTokenStore { get; } = refreshTokenStore;
        public ReviewStore ReviewStore { get; } = reviewStore;
        public ReviewOrderStore ReviewOrderStore { get; } = reviewOrderStore;
        public TransactionStore TransactionStore { get; } = transactionStore;
        public UserEntitlementStore UserEntitlementStore { get; } = userEntitlementStore;
        public UserNicknameStore UserNicknameStore { get; } = userNicknameStore;

        public Task<IDbContextTransaction> BeginTransaction(CancellationToken ct) => context.Database.BeginTransactionAsync(ct);
        public Task<int> SaveChanges(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Remove(entity);
    }
}
